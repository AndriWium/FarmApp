using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IStockBatchRepository
{
    Task<StockBatch?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<StockBatch, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<StockBatch, TResult>> selector, CancellationToken ct);
    Task AddAsync(StockBatch batch, CancellationToken ct);

    /// <summary>Batches created from the given ProducePurchaseLine ids - lets a purchase be read
    /// back with each line's resulting StockBatchId for traceability (doc 02).</summary>
    Task<List<TResult>> GetByPurchaseLineIdsAsync<TResult>(
        IEnumerable<int> purchaseLineIds, Expression<Func<StockBatch, TResult>> selector, CancellationToken ct);

    /// <summary>UnitCost for each of the given batch ids, keyed by StockBatchId - what
    /// IStockMovementService.RecordSaleDepletionAsync needs after FIFO allocation to hand the
    /// caller each touched batch's cost (StockAllocation itself only carries BatchId/QtyToTake).</summary>
    Task<Dictionary<int, decimal>> GetUnitCostsByIdsAsync(IEnumerable<int> batchIds, CancellationToken ct);
}
