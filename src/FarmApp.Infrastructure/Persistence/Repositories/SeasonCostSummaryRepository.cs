using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class SeasonCostSummaryRepository(FarmAppDbContext db) : ISeasonCostSummaryRepository
{
    public Task<SeasonCostSummary?> GetBySeasonIdAsync(int seasonId, CancellationToken ct)
        => db.SeasonCostSummaries.AsNoTracking().FirstOrDefaultAsync(x => x.SeasonId == seasonId, ct);

    public async Task AddAsync(SeasonCostSummary summary, CancellationToken ct)
        => await db.SeasonCostSummaries.AddAsync(summary, ct);
}
