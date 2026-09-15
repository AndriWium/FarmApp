using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class HarvestRepository(FarmAppDbContext db) : IHarvestRepository
{
    public Task<Harvest?> GetByIdAsync(int id, CancellationToken ct)
        => db.Harvests.FirstOrDefaultAsync(x => x.HarvestId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Harvest, TResult>> selector, CancellationToken ct)
        => db.Harvests.AsNoTracking()
            .Where(x => x.HarvestId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Harvest, TResult>> selector, int? seasonId, CancellationToken ct)
        => db.Harvests.AsNoTracking()
            .Where(x => seasonId == null || x.SeasonId == seasonId)
            .OrderByDescending(x => x.Date)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Harvest harvest, CancellationToken ct)
        => await db.Harvests.AddAsync(harvest, ct);
}
