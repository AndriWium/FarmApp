using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.StockMovements;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;
using FarmApp.Domain.Services;

namespace FarmApp.Api.Application.Sales;

public class SaleService(
    ISaleRepository saleRepo,
    ISaleLineRepository saleLineRepo,
    ISalePaymentRepository salePaymentRepo,
    ITillSessionRepository tillSessionRepo,
    ICustomerRepository customerRepo,
    IProductRepository productRepo,
    IGradeRepository gradeRepo,
    IPackSizeRepository packSizeRepo,
    IStockMovementService stockMovementService,
    ISaleLineCalculator saleLineCalculator,
    IUnitOfWork uow) : ISaleService
{
    public async Task<SaleDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var sale = await saleRepo.GetByIdAsync(id, ct);
        return sale is null ? null : await ToDtoAsync(sale, ct);
    }

    public async Task<List<SaleDto>> GetAllAsync(int? tillSessionId, CancellationToken ct)
    {
        var sales = await saleRepo.GetAllAsync(s => s, tillSessionId, ct);
        var result = new List<SaleDto>();
        foreach (var sale in sales)
            result.Add(await ToDtoAsync(sale, ct));
        return result;
    }

    public async Task<ServiceResult<CreateSaleResult>> CreateSaleAsync(CreateSaleRequest request, CancellationToken ct)
    {
        // 1. Idempotency, first thing, before anything else is even validated (doc 08): a retried
        // POST with a ClientGuid we've already processed returns the existing sale unchanged -
        // never reprocessed, never re-depletes stock. This is what makes an offline-retry safe.
        // WasReplay: true tells the controller to answer 200, not 201 (doc 08's own example).
        var existing = await saleRepo.GetByClientGuidAsync(request.ClientGuid, ct);
        if (existing is not null)
            return ServiceResult<CreateSaleResult>.Ok(new CreateSaleResult(await ToDtoAsync(existing, ct), WasReplay: true));

        // 2. The till session must exist and be open - a sale can't happen against a closed till.
        var till = await tillSessionRepo.GetByIdAsync(request.TillSessionId, ct);
        if (till is null) return ServiceResult<CreateSaleResult>.Fail(ServiceError.NotFound);
        if (till.ClosedAt is not null) return ServiceResult<CreateSaleResult>.Fail(ServiceError.TillSessionClosed);

        // 3. Customer, if supplied, must exist.
        if (request.CustomerId is not null && await customerRepo.GetByIdAsync(request.CustomerId.Value, ct) is null)
            return ServiceResult<CreateSaleResult>.Fail(ServiceError.NotFound);

        // 4. Every line's Product/Grade/PackSize must exist and (for PackSize) actually belong to
        // that line's Product - validated up front, before any write, same discipline as
        // ProducePurchaseService (see DECISIONS.md).
        foreach (var line in request.Lines)
        {
            if (await productRepo.GetByIdAsync(line.ProductId, ct) is null)
                return ServiceResult<CreateSaleResult>.Fail(ServiceError.NotFound);

            if (line.GradeId is not null && await gradeRepo.GetByIdAsync(line.GradeId.Value, ct) is null)
                return ServiceResult<CreateSaleResult>.Fail(ServiceError.NotFound);

            if (line.PackSizeId is not null)
            {
                var packSize = await packSizeRepo.GetByIdAsync(line.PackSizeId.Value, p => new { p.ProductId }, ct);
                if (packSize is null || packSize.ProductId != line.ProductId)
                    return ServiceResult<CreateSaleResult>.Fail(ServiceError.NotFound);
            }
        }

        // 5. An Account payment always needs a customer to put the debt against - no anonymous
        // walk-in sale can go on account.
        if (request.Payments.Any(p => p.Method == SalePaymentMethod.Account) && request.CustomerId is null)
            return ServiceResult<CreateSaleResult>.Fail(ServiceError.AccountPaymentRequiresCustomer);

        // 6. Payments must exactly cover the computed line total - no partial/short payments this
        // phase (task brief). The line total only depends on the request itself (Qty/UnitPrice/
        // DiscountAmount), not on stock, so this is fully checkable before the transaction opens.
        var lineTotal = request.Lines.Sum(l => (l.Qty * l.UnitPrice) - l.DiscountAmount);
        var paymentTotal = request.Payments.Sum(p => p.Amount);
        if (paymentTotal != lineTotal)
            return ServiceResult<CreateSaleResult>.Fail(ServiceError.PaymentMismatch,
                $"Payments total {paymentTotal:0.00}, sale total is {lineTotal:0.00}.");

        // Everything above is deterministic from the request alone; only stock availability can
        // still legitimately fail from here (a concurrent sale could deplete a batch between this
        // point and the FIFO allocation below) - so the real transaction starts now. Unlike
        // ProducePurchaseService/StockBatchService's two-phase-save stopgap (sequential
        // SaveChangesAsync calls with only pre-validation narrowing the failure window - see
        // DECISIONS.md), this wraps every write in a genuine EF Core transaction: if depletion
        // fails partway through, the Sale header already flushed by the SaveChangesAsync below is
        // rolled back too, not just abandoned in memory.
        await uow.BeginTransactionAsync(ct);
        try
        {
            var sale = new Sale
            {
                TillSessionId = request.TillSessionId,
                CustomerId = request.CustomerId,
                DateTime = DateTime.UtcNow,
                Channel = request.Channel,
                Status = SaleStatus.Complete,
                Notes = request.Notes,
                ClientGuid = request.ClientGuid,
            };
            await saleRepo.AddAsync(sale, ct);
            // Materialize SaleId before it's used below as StockMovement.RefId and every line's/
            // payment's SaleId FK (plain FK columns, no navigation - matches the rest of the
            // codebase's loose-FK convention).
            await uow.SaveChangesAsync(ct);

            var saleLines = new List<SaleLine>();
            foreach (var line in request.Lines)
            {
                var packSize = line.PackSizeId is null
                    ? null
                    : await packSizeRepo.GetByIdAsync(line.PackSizeId.Value, p => new { p.QtyInBaseUnit }, ct);

                // The unit-of-measure rule (task brief - the easiest thing here to get subtly
                // wrong): Qty counts packs when PackSizeId is set, so the quantity actually
                // depleted from stock is Qty x QtyInBaseUnit; Qty is already in the product's
                // base unit when PackSizeId is null. UnitPrice needs no such conversion - the
                // line total is always simply Qty x UnitPrice - DiscountAmount.
                var baseUnitQty = saleLineCalculator.ToBaseUnitQty(line.Qty, packSize?.QtyInBaseUnit);

                var depletion = await stockMovementService.RecordSaleDepletionAsync(
                    line.ProductId, line.GradeId, baseUnitQty, sale.SaleId, till.LocationId, ct);

                if (depletion.Error != ServiceError.None)
                {
                    // Insufficient stock on this line - the whole sale rolls back, including the
                    // Sale header already saved above and any earlier line's already-added
                    // movements: nothing about this sale survives (task brief's check #4).
                    await uow.RollbackTransactionAsync(ct);
                    return ServiceResult<CreateSaleResult>.Fail(depletion.Error, depletion.Detail!);
                }

                // Flush this line's movements now, before the next line's FIFO allocation query
                // runs. IStockMovementRepository.GetAvailableBatchesAsync re-reads on-hand from
                // the database (not the change tracker) - without this, two lines selling the
                // same product/grade would each compute availability from the same starting
                // snapshot and could double-allocate the same batch capacity (caught in testing:
                // a 12kg line fully draining a 5kg batch, followed by a 1.5kg line that should
                // spill into the next batch, instead "found" 5kg still sitting in the first batch
                // because its depleting movements hadn't been flushed yet). Still inside the one
                // transaction opened above, so this doesn't weaken atomicity - a later line's
                // failure still rolls every earlier flush back too.
                await uow.SaveChangesAsync(ct);

                // A sale line can legitimately span two batches at different costs - CostAtSale is
                // the qty-weighted average across whichever batches FIFO actually touched, never
                // just the first batch's cost (task brief), computed by the Domain's pure,
                // unit-tested calculator.
                var costAtSale = saleLineCalculator.WeightedAverageCost(
                    depletion.Value!.Allocations
                        .Select(a => new CostedAllocation(a.QtyTaken, a.UnitCost))
                        .ToList());

                saleLines.Add(new SaleLine
                {
                    SaleId = sale.SaleId,
                    ProductId = line.ProductId,
                    GradeId = line.GradeId,
                    PackSizeId = line.PackSizeId,
                    Qty = line.Qty,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount,
                    DiscountReason = line.DiscountReason,
                    CostAtSale = costAtSale,
                });
            }
            await saleLineRepo.AddRangeAsync(saleLines, ct);

            var payments = request.Payments.Select(p => new SalePayment
            {
                SaleId = sale.SaleId,
                Method = p.Method,
                Amount = p.Amount,
            }).ToList();
            await salePaymentRepo.AddRangeAsync(payments, ct);

            // One save for every line's stock movements + every SaleLine + every SalePayment,
            // then commit - the sale, its stock depletion, and its payments become visible
            // together or not at all.
            await uow.SaveChangesAsync(ct);
            await uow.CommitTransactionAsync(ct);

            return ServiceResult<CreateSaleResult>.Ok(new CreateSaleResult(await ToDtoAsync(sale, ct), WasReplay: false));
        }
        catch
        {
            await uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<SaleDto> ToDtoAsync(Sale sale, CancellationToken ct)
    {
        var lines = await saleLineRepo.GetBySaleIdAsync(sale.SaleId, l => new SaleLineDto(
            l.SaleLineId, l.ProductId, l.GradeId, l.PackSizeId, l.Qty, l.UnitPrice,
            l.DiscountAmount, l.DiscountReason, l.CostAtSale), ct);

        var payments = await salePaymentRepo.GetBySaleIdAsync(sale.SaleId, p => new SalePaymentDto(
            p.SalePaymentId, p.Method, p.Amount), ct);

        return new SaleDto(
            sale.SaleId, sale.TillSessionId, sale.CustomerId, sale.DateTime, sale.Channel,
            sale.Status, sale.Notes, sale.ClientGuid, lines, payments);
    }
}
