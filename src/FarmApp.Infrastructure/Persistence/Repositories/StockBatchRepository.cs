using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class StockBatchRepository(FarmAppDbContext db) : IStockBatchRepository
{
    public Task<StockBatch?> GetByIdAsync(int id, CancellationToken ct)
        => db.StockBatches.FirstOrDefaultAsync(x => x.StockBatchId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<StockBatch, TResult>> selector, CancellationToken ct)
        => db.StockBatches.AsNoTracking()
            .Where(x => x.StockBatchId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<StockBatch, TResult>> selector, CancellationToken ct)
        => db.StockBatches.AsNoTracking()
            .OrderByDescending(x => x.Date)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(StockBatch batch, CancellationToken ct)
        => await db.StockBatches.AddAsync(batch, ct);
}
