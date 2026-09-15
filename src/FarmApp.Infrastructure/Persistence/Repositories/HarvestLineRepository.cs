using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class HarvestLineRepository(FarmAppDbContext db) : IHarvestLineRepository
{
    public Task<List<TResult>> GetByHarvestIdAsync<TResult>(
        int harvestId, Expression<Func<HarvestLine, TResult>> selector, CancellationToken ct)
        => db.HarvestLines.AsNoTracking()
            .Where(x => x.HarvestId == harvestId)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<HarvestLine> lines, CancellationToken ct)
        => await db.HarvestLines.AddRangeAsync(lines, ct);
}
