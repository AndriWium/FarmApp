using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class SeasonRepository(FarmAppDbContext db) : ISeasonRepository
{
    public Task<Season?> GetByIdAsync(int id, CancellationToken ct)
        => db.Seasons.FirstOrDefaultAsync(x => x.SeasonId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Season, TResult>> selector, CancellationToken ct)
        => db.Seasons.AsNoTracking()
            .Where(x => x.SeasonId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Season, TResult>> selector, int? plantingId, CancellationToken ct)
        => db.Seasons.AsNoTracking()
            .Where(x => plantingId == null || x.PlantingId == plantingId)
            .OrderByDescending(x => x.StartDate)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Season season, CancellationToken ct)
        => await db.Seasons.AddAsync(season, ct);

    public Task<List<Season>> GetSeasonsOverlappingPeriodAsync(int year, int month, CancellationToken ct)
    {
        var periodStart = new DateTime(year, month, 1);
        var periodEnd = periodStart.AddMonths(1);
        return db.Seasons.AsNoTracking()
            .Where(x => x.StartDate < periodEnd && x.EndDate >= periodStart)
            .ToListAsync(ct);
    }
}
