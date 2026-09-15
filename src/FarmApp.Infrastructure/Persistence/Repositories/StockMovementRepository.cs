using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using FarmApp.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class StockMovementRepository(FarmAppDbContext db) : IStockMovementRepository
{
    public async Task<decimal> GetOnHandAsync(int stockBatchId, CancellationToken ct)
        => await db.StockMovements.AsNoTracking()
            .Where(m => m.StockBatchId == stockBatchId)
            .SumAsync(m => (decimal?)m.Qty, ct) ?? 0m;

    public async Task<IReadOnlyList<BatchAvailability>> GetAvailableBatchesAsync(int productId, int? gradeId, CancellationToken ct)
    {
        var batches = await db.StockBatches.AsNoTracking()
            .Where(b => b.ProductId == productId && b.GradeId == gradeId)
            .OrderBy(b => b.Date)
            .ThenBy(b => b.StockBatchId)
            .Select(b => new { b.StockBatchId, b.Date })
            .ToListAsync(ct);

        if (batches.Count == 0)
            return Array.Empty<BatchAvailability>();

        var batchIds = batches.Select(b => b.StockBatchId).ToList();

        var onHandByBatch = await db.StockMovements.AsNoTracking()
            .Where(m => batchIds.Contains(m.StockBatchId))
            .GroupBy(m => m.StockBatchId)
            .Select(g => new { StockBatchId = g.Key, OnHand = g.Sum(m => m.Qty) })
            .ToDictionaryAsync(x => x.StockBatchId, x => x.OnHand, ct);

        return batches
            .Select(b => new BatchAvailability(
                b.StockBatchId, b.Date, onHandByBatch.GetValueOrDefault(b.StockBatchId, 0m)))
            .Where(a => a.QtyAvailable > 0)
            .ToList();
    }

    public async Task<List<StockOnHandRow>> GetOnHandSummaryAsync(CancellationToken ct)
    {
        // EF Core can't translate a GroupBy+Sum whose key/aggregates reach back through multiple
        // joins' transparent identifiers into a record constructor server-side - and composing
        // a "flat projection, then GroupBy" over the same IQueryable doesn't help, since nothing
        // forces evaluation in between: it's still one expression tree at translation time. So
        // the join is materialized here (ToListAsync) and the grouping happens client-side over
        // the flat rows instead - fine at this phase's data volumes (a small farm's whole
        // movement ledger), and simpler than hand-rolling SQL for one summary query.
        var flat = await (
            from m in db.StockMovements.AsNoTracking()
            join b in db.StockBatches.AsNoTracking() on m.StockBatchId equals b.StockBatchId
            join p in db.Products.AsNoTracking() on b.ProductId equals p.ProductId
            join g in db.Grades.AsNoTracking() on b.GradeId equals g.GradeId into grades
            from g in grades.DefaultIfEmpty()
            select new { b.ProductId, ProductName = p.Name, b.GradeId, GradeName = (string?)g.Name, m.Qty, b.UnitCost }
        ).ToListAsync(ct);

        return flat
            .GroupBy(x => new { x.ProductId, x.ProductName, x.GradeId, x.GradeName })
            .Select(grp => new StockOnHandRow(
                grp.Key.ProductId,
                grp.Key.ProductName,
                grp.Key.GradeId,
                grp.Key.GradeName,
                grp.Sum(x => x.Qty),
                grp.Sum(x => x.Qty * x.UnitCost)))
            .OrderBy(r => r.ProductName)
            .ThenBy(r => r.GradeName)
            .ToList();
    }

    public async Task AddRangeAsync(IEnumerable<StockMovement> movements, CancellationToken ct)
        => await db.StockMovements.AddRangeAsync(movements, ct);
}
