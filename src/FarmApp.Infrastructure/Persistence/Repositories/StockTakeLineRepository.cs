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
}
