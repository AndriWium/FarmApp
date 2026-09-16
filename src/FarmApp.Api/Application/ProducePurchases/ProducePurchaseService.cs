using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.StockBatches;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.ProducePurchases;

public class ProducePurchaseService(
    IProducePurchaseRepository repo,
    IProducePurchaseLineRepository lineRepo,
    IStockBatchRepository stockBatchRepo,
    ISupplierRepository supplierRepo,
    IProductRepository productRepo,
    IGradeRepository gradeRepo,
    IStockBatchService stockBatchService,
    IUnitOfWork uow) : IProducePurchaseService
{
    public async Task<List<ProducePurchaseDto>> GetAllAsync(CancellationToken ct)
    {
        var purchases = await repo.GetAllAsync(p => p, ct);
        var result = new List<ProducePurchaseDto>();
        foreach (var purchase in purchases)
            result.Add(await ToDtoAsync(purchase, ct));
        return result;
    }

    public async Task<ProducePurchaseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var purchase = await repo.GetByIdAsync(id, ct);
        return purchase is null ? null : await ToDtoAsync(purchase, ct);
    }

    public async Task<ServiceResult<ProducePurchaseDto>> CreatePurchaseAsync(CreatePurchaseRequest request, CancellationToken ct)
    {
        // Validate the supplier and every line's product/grade before writing anything - a
        // failure partway through a multi-entity operation (header, N lines, N batches, N
        // movements) must never happen silently (task brief).
        if (await supplierRepo.GetByIdAsync(request.SupplierId, ct) is null)
            return ServiceResult<ProducePurchaseDto>.Fail(ServiceError.NotFound);

        foreach (var line in request.Lines)
        {
            if (await productRepo.GetByIdAsync(line.ProductId, ct) is null)
                return ServiceResult<ProducePurchaseDto>.Fail(ServiceError.NotFound);

            if (line.GradeId is not null && await gradeRepo.GetByIdAsync(line.GradeId.Value, ct) is null)
                return ServiceResult<ProducePurchaseDto>.Fail(ServiceError.NotFound);
        }

        // Everything above is deterministic from the request alone; only the per-line StockBatch
        // creation can still legitimately fail from here (defensively - every product/grade was
        // already validated) - so the real transaction starts now. Same genuine-EF-Core-transaction
        // pattern as HarvestService/ActivityService/SaleService, replacing this method's older
        // two-phase-save stopgap (see DECISIONS.md / IUnitOfWork's own doc comment): a mid-operation
        // failure - including from ProducePurchase/ProducePurchaseLine now also being IPeriodLocked,
        // which can only ever fail on the very first SaveChangesAsync below - now rolls back
        // everything, including the header and line inserts, not just what StockBatchService itself
        // adds. StockBatchService.CreateAsync opens no transaction of its own, so it safely
        // participates in this ambient one.
        await uow.BeginTransactionAsync(ct);
        try
        {
            var purchase = new ProducePurchase
            {
                SupplierId = request.SupplierId,
                Date = request.Date,
                InvoiceRef = request.InvoiceRef,
            };
            await repo.AddAsync(purchase, ct);
            // Materialize ProducePurchaseId before the lines below can reference it - same loose-FK
            // two-phase-save shape StockBatchService itself still uses internally (see DECISIONS.md),
            // now inside one transaction rather than as this method's own outer atomicity boundary.
            await uow.SaveChangesAsync(ct);

            var lines = request.Lines.Select(l => new ProducePurchaseLine
            {
                ProducePurchaseId = purchase.ProducePurchaseId,
                ProductId = l.ProductId,
                GradeId = l.GradeId,
                Qty = l.Qty,
                UnitCost = l.UnitCost,
                VatAmount = l.VatAmount,
            }).ToList();
            await lineRepo.AddRangeAsync(lines, ct);
            // Materialize each ProducePurchaseLineId before it's used below as StockBatch.PurchaseLineId.
            await uow.SaveChangesAsync(ct);

            // One StockBatch (+ seeding PurchaseIn movement) per line, reusing StockBatchService
            // rather than duplicating its batch+movement creation logic (task brief). Every
            // product/grade reference was already validated above, so this is expected to always
            // succeed - defensively roll back the whole purchase if it somehow doesn't, rather than
            // leave a partially-built purchase behind.
            var lineDtos = new List<ProducePurchaseLineDto>();
            foreach (var (line, lineRequest) in lines.Zip(request.Lines))
            {
                var batchResult = await stockBatchService.CreateAsync(new CreateStockBatchRequest(
                    line.ProductId, line.GradeId, StockSource.Purchase,
                    HarvestId: null, PurchaseLineId: line.ProducePurchaseLineId,
                    purchase.Date, line.Qty, line.UnitCost, lineRequest.ShelfLifeDays), ct);

                if (batchResult.Error != ServiceError.None)
                {
                    await uow.RollbackTransactionAsync(ct);
                    return ServiceResult<ProducePurchaseDto>.Fail(batchResult.Error, batchResult.Detail ?? "Failed to create stock batch for purchase line.");
                }

                lineDtos.Add(new ProducePurchaseLineDto(
                    line.ProducePurchaseLineId, line.ProductId, line.GradeId, line.Qty, line.UnitCost, line.VatAmount,
                    batchResult.Value!.StockBatchId));
            }

            await uow.CommitTransactionAsync(ct);

            return ServiceResult<ProducePurchaseDto>.Ok(
                new ProducePurchaseDto(purchase.ProducePurchaseId, purchase.SupplierId, purchase.Date, purchase.InvoiceRef, lineDtos));
        }
        catch
        {
            await uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<ProducePurchaseDto> ToDtoAsync(ProducePurchase purchase, CancellationToken ct)
    {
        var lines = await lineRepo.GetByPurchaseIdAsync(purchase.ProducePurchaseId,
            l => new { l.ProducePurchaseLineId, l.ProductId, l.GradeId, l.Qty, l.UnitCost, l.VatAmount }, ct);

        var lineIds = lines.Select(l => l.ProducePurchaseLineId).ToList();
        var batches = await stockBatchRepo.GetByPurchaseLineIdsAsync(
            lineIds, b => new { b.PurchaseLineId, b.StockBatchId }, ct);
        var batchIdByLine = batches.ToDictionary(b => b.PurchaseLineId!.Value, b => b.StockBatchId);

        var lineDtos = lines
            .Select(l => new ProducePurchaseLineDto(
                l.ProducePurchaseLineId, l.ProductId, l.GradeId, l.Qty, l.UnitCost, l.VatAmount,
                batchIdByLine.GetValueOrDefault(l.ProducePurchaseLineId)))
            .ToList();

        return new ProducePurchaseDto(purchase.ProducePurchaseId, purchase.SupplierId, purchase.Date, purchase.InvoiceRef, lineDtos);
    }
}
