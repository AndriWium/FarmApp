using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;
using FarmApp.Domain.Services;

namespace FarmApp.Api.Application.StockMovements;

public class StockMovementService(
    IStockMovementRepository movementRepo,
    IStockAllocationService allocationService,
    IProductRepository productRepo,
    IGradeRepository gradeRepo,
    ILocationRepository locationRepo,
    IUnitOfWork uow) : IStockMovementService
{
    public async Task<ServiceResult<List<StockMovementDto>>> RecordAsync(
        StockMovementType type, RecordStockMovementRequest request, CancellationToken ct)
    {
        if (await productRepo.GetByIdAsync(request.ProductId, ct) is null)
            return ServiceResult<List<StockMovementDto>>.Fail(ServiceError.NotFound);

        if (request.GradeId is not null && await gradeRepo.GetByIdAsync(request.GradeId.Value, ct) is null)
            return ServiceResult<List<StockMovementDto>>.Fail(ServiceError.NotFound);

        if (request.LocationId is not null && await locationRepo.GetByIdAsync(request.LocationId.Value, ct) is null)
            return ServiceResult<List<StockMovementDto>>.Fail(ServiceError.NotFound);

        var allocated = await TryAllocateAsync(request.ProductId, request.GradeId, request.Qty, ct);
        if (allocated.Error != ServiceError.None)
            return ServiceResult<List<StockMovementDto>>.Fail(allocated.Error, allocated.Detail!);

        var now = DateTime.UtcNow;
        var movements = allocated.Value!.Select(a => new StockMovement
        {
            StockBatchId = a.BatchId,
            Date = now,
            Type = type,
            Qty = -a.QtyToTake, // Wastage/OwnUse/Sample/Donation/Adjustment/Repack always deplete — always negative
            Reason = request.Reason,
            LocationId = request.LocationId,
        }).ToList();

        await movementRepo.AddRangeAsync(movements, ct);
        await uow.SaveChangesAsync(ct); // one SaveChangesAsync for the whole operation — atomic

        return ServiceResult<List<StockMovementDto>>.Ok(movements.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<List<StockMovementDto>>> TransferAsync(TransferStockRequest request, CancellationToken ct)
    {
        if (await productRepo.GetByIdAsync(request.ProductId, ct) is null)
            return ServiceResult<List<StockMovementDto>>.Fail(ServiceError.NotFound);

        if (request.GradeId is not null && await gradeRepo.GetByIdAsync(request.GradeId.Value, ct) is null)
            return ServiceResult<List<StockMovementDto>>.Fail(ServiceError.NotFound);

        if (await locationRepo.GetByIdAsync(request.FromLocationId, ct) is null)
            return ServiceResult<List<StockMovementDto>>.Fail(ServiceError.NotFound);

        if (await locationRepo.GetByIdAsync(request.ToLocationId, ct) is null)
            return ServiceResult<List<StockMovementDto>>.Fail(ServiceError.NotFound);

        var allocated = await TryAllocateAsync(request.ProductId, request.GradeId, request.Qty, ct);
        if (allocated.Error != ServiceError.None)
            return ServiceResult<List<StockMovementDto>>.Fail(allocated.Error, allocated.Detail!);

        var now = DateTime.UtcNow;
        var movements = new List<StockMovement>();
        foreach (var a in allocated.Value!)
        {
            // A transfer doesn't consume stock — it produces a TransferOut at the source and a
            // TransferIn at the destination for the same batch/quantity, atomically (per batch
            // touched, since a transfer can span more than one FIFO batch just like any other
            // depletion). The batch's own net on-hand is unaffected (-qty + qty).
            movements.Add(new StockMovement
            {
                StockBatchId = a.BatchId, Date = now, Type = StockMovementType.TransferOut,
                Qty = -a.QtyToTake, Reason = request.Reason, LocationId = request.FromLocationId,
            });
            movements.Add(new StockMovement
            {
                StockBatchId = a.BatchId, Date = now, Type = StockMovementType.TransferIn,
                Qty = a.QtyToTake, Reason = request.Reason, LocationId = request.ToLocationId,
            });
        }

        await movementRepo.AddRangeAsync(movements, ct);
        await uow.SaveChangesAsync(ct); // one SaveChangesAsync for the whole operation — atomic

        return ServiceResult<List<StockMovementDto>>.Ok(movements.Select(ToDto).ToList());
    }

    public async Task<StockMovementDto> RecordBatchAdjustmentAsync(
        int stockBatchId, decimal signedQty, string? reason, int? locationId, CancellationToken ct)
    {
        var movement = new StockMovement
        {
            StockBatchId = stockBatchId,
            Date = DateTime.UtcNow,
            Type = StockMovementType.Adjustment,
            Qty = signedQty, // signed by the caller - it already knows the direction (stock-take variance)
            Reason = reason,
            LocationId = locationId,
        };
        await movementRepo.AddRangeAsync([movement], ct);
        // No SaveChangesAsync here by design - see the XML doc on IStockMovementService's
        // RecordBatchAdjustmentAsync. The caller (StockTakeService) saves once for the whole
        // reconciliation batch.
        return ToDto(movement);
    }

    public async Task<List<StockOnHandSummaryDto>> GetOnHandSummaryAsync(CancellationToken ct)
    {
        var rows = await movementRepo.GetOnHandSummaryAsync(ct);
        return rows.Select(r => new StockOnHandSummaryDto(
            r.ProductId, r.ProductName, r.GradeId, r.GradeName, r.QtyOnHand, r.Value)).ToList();
    }

    /// <summary>Fetches available batches for a product/grade and asks the Domain FIFO service
    /// to allocate quantityNeeded across them, translating InsufficientStockException into a
    /// ServiceResult so callers never need to catch a Domain exception themselves.</summary>
    private async Task<ServiceResult<IReadOnlyList<StockAllocation>>> TryAllocateAsync(
        int productId, int? gradeId, decimal quantityNeeded, CancellationToken ct)
    {
        var availableBatches = await movementRepo.GetAvailableBatchesAsync(productId, gradeId, ct);
        try
        {
            return ServiceResult<IReadOnlyList<StockAllocation>>.Ok(
                allocationService.AllocateFifo(availableBatches, quantityNeeded));
        }
        catch (InsufficientStockException ex)
        {
            return ServiceResult<IReadOnlyList<StockAllocation>>.Fail(
                ServiceError.InsufficientStock,
                $"Requested {ex.Requested}, only {ex.Available} on hand for this product/grade.");
        }
    }

    private static StockMovementDto ToDto(StockMovement m)
        => new(m.StockMovementId, m.StockBatchId, m.Date, m.Type, m.Qty, m.RefTable, m.RefId, m.Reason, m.LocationId);
}
