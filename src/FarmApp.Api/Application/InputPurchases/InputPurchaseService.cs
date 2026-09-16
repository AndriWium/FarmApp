using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.InputPurchases;

public class InputPurchaseService(
    IInputPurchaseRepository repo,
    IInputPurchaseLineRepository lineRepo,
    IInputStockMovementRepository movementRepo,
    ISupplierRepository supplierRepo,
    IInputItemRepository inputItemRepo,
    IUnitOfWork uow) : IInputPurchaseService
{
    public async Task<List<InputPurchaseDto>> GetAllAsync(CancellationToken ct)
    {
        var purchases = await repo.GetAllAsync(p => p, ct);
        var result = new List<InputPurchaseDto>();
        foreach (var purchase in purchases)
            result.Add(await ToDtoAsync(purchase, ct));
        return result;
    }

    public async Task<InputPurchaseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var purchase = await repo.GetByIdAsync(id, ct);
        return purchase is null ? null : await ToDtoAsync(purchase, ct);
    }

    public async Task<ServiceResult<InputPurchaseDto>> CreatePurchaseAsync(CreateInputPurchaseRequest request, CancellationToken ct)
    {
        // Validate the supplier and every line's input item before writing anything - a failure
        // partway through a multi-entity operation (header, N lines, N movements) must never
        // happen silently (task brief, matches ProducePurchaseService precedent).
        if (await supplierRepo.GetByIdAsync(request.SupplierId, ct) is null)
            return ServiceResult<InputPurchaseDto>.Fail(ServiceError.NotFound);

        foreach (var line in request.Lines)
        {
            if (await inputItemRepo.GetByIdAsync(line.InputItemId, ct) is null)
                return ServiceResult<InputPurchaseDto>.Fail(ServiceError.NotFound);
        }

        // Everything above is deterministic from the request alone; nothing past this point should
        // legitimately fail (every InputItemId was already validated) - so the real transaction
        // starts now. Same genuine-EF-Core-transaction pattern as HarvestService/ProducePurchase-
        // Service/ActivityService/SaleService, replacing this method's older two-phase-save stopgap
        // (see DECISIONS.md / IUnitOfWork's own doc comment): a mid-operation failure - including
        // from InputPurchase/InputPurchaseLine now also being IPeriodLocked, which can only ever
        // fail on the very first SaveChangesAsync below - now rolls back everything, including the
        // header and line inserts, not just whatever movement rows were still unflushed.
        await uow.BeginTransactionAsync(ct);
        try
        {
            var purchase = new InputPurchase
            {
                SupplierId = request.SupplierId,
                Date = request.Date,
                InvoiceRef = request.InvoiceRef,
            };
            await repo.AddAsync(purchase, ct);
            // Materialize InputPurchaseId before the lines below can reference it.
            await uow.SaveChangesAsync(ct);

            var lines = request.Lines.Select(l => new InputPurchaseLine
            {
                InputPurchaseId = purchase.InputPurchaseId,
                InputItemId = l.InputItemId,
                Qty = l.Qty,
                UnitCost = l.UnitCost,
                VatAmount = l.VatAmount,
            }).ToList();
            await lineRepo.AddRangeAsync(lines, ct);
            // Materialize each InputPurchaseLineId before it's used below as the seeding movement's RefId.
            await uow.SaveChangesAsync(ct);

            var movements = lines.Select(l => new InputStockMovement
            {
                InputItemId = l.InputItemId,
                Date = purchase.Date,
                Type = InputStockMovementType.PurchaseIn,
                Qty = l.Qty, // positive - stock coming in
                UnitCost = l.UnitCost,
                RefTable = "InputPurchaseLine",
                RefId = l.InputPurchaseLineId,
            }).ToList();
            await movementRepo.AddRangeAsync(movements, ct);
            await uow.SaveChangesAsync(ct);

            await uow.CommitTransactionAsync(ct);

            return ServiceResult<InputPurchaseDto>.Ok(new InputPurchaseDto(
                purchase.InputPurchaseId, purchase.SupplierId, purchase.Date, purchase.InvoiceRef,
                lines.Select(l => new InputPurchaseLineDto(l.InputPurchaseLineId, l.InputItemId, l.Qty, l.UnitCost, l.VatAmount)).ToList()));
        }
        catch
        {
            await uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<InputPurchaseDto> ToDtoAsync(InputPurchase purchase, CancellationToken ct)
    {
        var lines = await lineRepo.GetByPurchaseIdAsync(purchase.InputPurchaseId,
            l => new InputPurchaseLineDto(l.InputPurchaseLineId, l.InputItemId, l.Qty, l.UnitCost, l.VatAmount), ct);

        return new InputPurchaseDto(purchase.InputPurchaseId, purchase.SupplierId, purchase.Date, purchase.InvoiceRef, lines);
    }
}
