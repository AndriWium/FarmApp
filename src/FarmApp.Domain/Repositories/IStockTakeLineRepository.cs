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

    /// <summary>Every StockTakeLine with a non-zero Variance whose StockTake.Date falls in the
    /// given [year, month] - the month-end close checklist's stock-take-variance item (doc 10 §1
    /// item 3). "Material" is read simply as "non-zero" here (task brief: there's no "reviewed"/
    /// "actioned" flag anywhere in this schema to distinguish a variance a human has already
    /// looked at from one they haven't, so every non-zero variance in the period surfaces).</summary>
    Task<List<StockTakeVarianceRow>> GetNonZeroVariancesInPeriodAsync(int year, int month, CancellationToken ct);
}
