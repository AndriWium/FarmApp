using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class StockTakeLineRepository(FarmAppDbContext db) : IStockTakeLineRepository
{
    public Task<List<StockTakeLine>> GetByStockTakeIdAsync(int stockTakeId, CancellationToken ct)
        => db.StockTakeLines.Where(x => x.StockTakeId == stockTakeId).ToListAsync(ct);

    public Task<List<TResult>> GetByStockTakeIdAsync<TResult>(
        int stockTakeId, Expression<Func<StockTakeLine, TResult>> selector, CancellationToken ct)
        => db.StockTakeLines.AsNoTracking()
            .Where(x => x.StockTakeId == stockTakeId)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<StockTakeLine> lines, CancellationToken ct)
        => await db.StockTakeLines.AddRangeAsync(lines, ct);

    public Task<List<StockTakeVarianceRow>> GetNonZeroVariancesInPeriodAsync(int year, int month, CancellationToken ct)
    {
        var periodStart = new DateTime(year, month, 1);
        var periodEnd = periodStart.AddMonths(1);

        return (
            from line in db.StockTakeLines.AsNoTracking()
            join take in db.StockTakes.AsNoTracking() on line.StockTakeId equals take.StockTakeId
            where take.Date >= periodStart && take.Date < periodEnd && line.Variance != 0
            select new StockTakeVarianceRow(take.StockTakeId, take.Date, line.StockTakeLineId, line.StockBatchId, line.Variance)
        ).ToListAsync(ct);
    }
}
