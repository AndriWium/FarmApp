using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class StockTakeRepository(FarmAppDbContext db) : IStockTakeRepository
{
    public Task<StockTake?> GetByIdAsync(int id, CancellationToken ct)
        => db.StockTakes.FirstOrDefaultAsync(x => x.StockTakeId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<StockTake, TResult>> selector, CancellationToken ct)
        => db.StockTakes.AsNoTracking()
            .Where(x => x.StockTakeId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<StockTake, TResult>> selector, CancellationToken ct)
        => db.StockTakes.AsNoTracking()
            .OrderByDescending(x => x.Date)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(StockTake stockTake, CancellationToken ct)
        => await db.StockTakes.AddAsync(stockTake, ct);
}
