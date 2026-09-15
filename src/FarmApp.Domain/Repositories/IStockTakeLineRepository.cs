using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

/// <summary>Backs StockTake's line sub-resource. Deliberately no top-level CRUD surface - lines
/// are only ever created via IStockTakeService.StartStockTakeAsync and mutated in place by
/// RecordCountsAsync (matches IRecipeLineRepository's precedent).</summary>
public interface IStockTakeLineRepository
{
    /// <summary>Tracked - RecordCountsAsync mutates CountedQty/Variance on these in place.</summary>
    Task<List<StockTakeLine>> GetByStockTakeIdAsync(int stockTakeId, CancellationToken ct);

    Task<List<TResult>> GetByStockTakeIdAsync<TResult>(
        int stockTakeId, Expression<Func<StockTakeLine, TResult>> selector, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<StockTakeLine> lines, CancellationToken ct);
}
