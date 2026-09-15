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

        var purchase = new InputPurchase
        {
            SupplierId = request.SupplierId,
            Date = request.Date,
            InvoiceRef = request.InvoiceRef,
        };
        await repo.AddAsync(purchase, ct);
        // Materialize InputPurchaseId before the lines below can reference it - same two-phase-
        // save stopgap ProducePurchaseService already established (see DECISIONS.md).
        await uow.SaveChangesAsync(ct);

        var lines = request.Lines.Select(l => new InputPurchaseLine
        {
            InputPurchaseId = purchase.InputPurchaseId,
            InputItemId = l.InputItemId,
            Qty = l.Qty,
            UnitCost = l.UnitCost,
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

        return ServiceResult<InputPurchaseDto>.Ok(new InputPurchaseDto(
            purchase.InputPurchaseId, purchase.SupplierId, purchase.Date, purchase.InvoiceRef,
            lines.Select(l => new InputPurchaseLineDto(l.InputPurchaseLineId, l.InputItemId, l.Qty, l.UnitCost)).ToList()));
    }

    private async Task<InputPurchaseDto> ToDtoAsync(InputPurchase purchase, CancellationToken ct)
    {
        var lines = await lineRepo.GetByPurchaseIdAsync(purchase.InputPurchaseId,
            l => new InputPurchaseLineDto(l.InputPurchaseLineId, l.InputItemId, l.Qty, l.UnitCost), ct);

        return new InputPurchaseDto(purchase.InputPurchaseId, purchase.SupplierId, purchase.Date, purchase.InvoiceRef, lines);
    }
}
