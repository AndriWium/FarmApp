using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using FarmApp.Domain.Services;
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

    public Task<List<TResult>> GetByPurchaseLineIdsAsync<TResult>(
        IEnumerable<int> purchaseLineIds, Expression<Func<StockBatch, TResult>> selector, CancellationToken ct)
        => db.StockBatches.AsNoTracking()
            .Where(b => b.PurchaseLineId != null && purchaseLineIds.Contains(b.PurchaseLineId.Value))
            .Select(selector)
            .ToListAsync(ct);

    public Task<Dictionary<int, decimal>> GetUnitCostsByIdsAsync(IEnumerable<int> batchIds, CancellationToken ct)
        => db.StockBatches.AsNoTracking()
            .Where(b => batchIds.Contains(b.StockBatchId))
            .ToDictionaryAsync(b => b.StockBatchId, b => b.UnitCost, ct);

    public Task<List<TResult>> GetByHarvestIdAsync<TResult>(
        int harvestId, Expression<Func<StockBatch, TResult>> selector, CancellationToken ct)
        => db.StockBatches.AsNoTracking()
            .Where(b => b.HarvestId == harvestId)
            .OrderBy(b => b.StockBatchId)
            .Select(selector)
            .ToListAsync(ct);

    public Task<List<HarvestBatchCost>> GetHarvestBatchesBySeasonIdAsync(int seasonId, CancellationToken ct)
        => db.StockBatches.AsNoTracking()
            .Where(b => b.HarvestId != null && db.Harvests.Any(h => h.HarvestId == b.HarvestId && h.SeasonId == seasonId))
            .Select(b => new HarvestBatchCost(b.QtyIn, b.UnitCost))
            .ToListAsync(ct);
}
