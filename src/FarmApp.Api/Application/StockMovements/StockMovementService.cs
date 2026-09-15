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
    IStockBatchRepository stockBatchRepo,
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

    public async Task<ServiceResult<SaleDepletionResult>> RecordSaleDepletionAsync(
        int productId, int? gradeId, decimal qtyBaseUnits, int saleId, int? locationId, CancellationToken ct)
    {
        var allocated = await TryAllocateAsync(productId, gradeId, qtyBaseUnits, ct);
        if (allocated.Error != ServiceError.None)
            return ServiceResult<SaleDepletionResult>.Fail(
                allocated.Error, $"Product {productId}: {allocated.Detail}");

        // StockAllocation only carries BatchId/QtyToTake - fetch each touched batch's UnitCost so
        // the caller can compute a weighted-average CostAtSale without a second round trip itself.
        var batchIds = allocated.Value!.Select(a => a.BatchId).ToList();
        var unitCostByBatch = await stockBatchRepo.GetUnitCostsByIdsAsync(batchIds, ct);

        var now = DateTime.UtcNow;
        var movements = allocated.Value!.Select(a => new StockMovement
        {
            StockBatchId = a.BatchId,
            Date = now,
            Type = StockMovementType.SaleOut,
            Qty = -a.QtyToTake, // depletion is always negative
            RefTable = "Sale",
            RefId = saleId,
            LocationId = locationId,
        }).ToList();

        await movementRepo.AddRangeAsync(movements, ct);
        // No SaveChangesAsync here by design - see the XML doc on IStockMovementService's
        // RecordSaleDepletionAsync (same contract as RecordBatchAdjustmentAsync). SaleService
        // saves and commits once for the whole sale (header, every line's movements, every
        // payment) inside its own explicit transaction.

        var allocations = allocated.Value!
            .Select(a => new SaleDepletionAllocation(a.BatchId, a.QtyToTake, unitCostByBatch[a.BatchId]))
            .ToList();

        return ServiceResult<SaleDepletionResult>.Ok(
            new SaleDepletionResult(allocations, movements.Select(ToDto).ToList()));
    }

    public async Task<List<StockMovementDto>> ReverseSaleDepletionAsync(int saleId, string? reason, CancellationToken ct)
    {
        var original = await movementRepo.GetBySaleDepletionAsync(saleId, ct);

        var now = DateTime.UtcNow;
        var reversalReason = reason is null ? $"Refund of Sale {saleId}" : $"Refund of Sale {saleId}: {reason}";
        var reversals = original.Select(m => new StockMovement
        {
            StockBatchId = m.StockBatchId,
            Date = now,
            Type = StockMovementType.Adjustment,
            Qty = -m.Qty, // original SaleOut Qty is negative (depletion); crediting back is the same magnitude, positive
            RefTable = "Sale",
            RefId = saleId,
            Reason = reversalReason,
            LocationId = m.LocationId,
        }).ToList();

        await movementRepo.AddRangeAsync(reversals, ct);
        // No SaveChangesAsync here by design - see the XML doc on IStockMovementService's
        // ReverseSaleDepletionAsync. SaleService.RefundSaleAsync saves and commits once for the
        // whole refund (every reversal movement + Sale.Status) inside its own explicit transaction.

        return reversals.Select(ToDto).ToList();
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
