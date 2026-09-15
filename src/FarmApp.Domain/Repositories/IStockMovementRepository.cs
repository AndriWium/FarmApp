using FarmApp.Domain.Entities;
using FarmApp.Domain.Services;

namespace FarmApp.Domain.Repositories;

public interface IStockMovementRepository
{
    /// <summary>SUM(Qty) for this batch - the on-hand invariant (doc 02): never a stored column,
    /// always derived from the movement ledger.</summary>
    Task<decimal> GetOnHandAsync(int stockBatchId, CancellationToken ct);

    /// <summary>Batches for this product/grade with on-hand > 0, ordered oldest-first by
    /// StockBatch.Date - exactly the shape IStockAllocationService.AllocateFifo needs.</summary>
    Task<IReadOnlyList<BatchAvailability>> GetAvailableBatchesAsync(int productId, int? gradeId, CancellationToken ct);

    /// <summary>On-hand quantity and value, grouped by Product/Grade.</summary>
    Task<List<StockOnHandRow>> GetOnHandSummaryAsync(CancellationToken ct);

    /// <summary>Inserts one or more movement rows. The caller (a use-case service) is
    /// responsible for calling IUnitOfWork.SaveChangesAsync once for the whole operation.</summary>
    Task AddRangeAsync(IEnumerable<StockMovement> movements, CancellationToken ct);
}
