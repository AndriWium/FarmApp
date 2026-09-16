using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class ActivityRepository(FarmAppDbContext db) : IActivityRepository
{
    public Task<Activity?> GetByIdAsync(int id, CancellationToken ct)
        => db.Activities.FirstOrDefaultAsync(x => x.ActivityId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Activity, TResult>> selector, CancellationToken ct)
        => db.Activities.AsNoTracking()
            .Where(x => x.ActivityId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Activity, TResult>> selector, int? seasonId, CancellationToken ct)
        => db.Activities.AsNoTracking()
            .Where(x => seasonId == null || x.SeasonId == seasonId)
            .OrderByDescending(x => x.Date)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Activity activity, CancellationToken ct)
        => await db.Activities.AddAsync(activity, ct);

    public Task<decimal> GetTotalLabourCostForSeasonAsync(int seasonId, CancellationToken ct)
        => db.Activities.AsNoTracking()
            .Where(a => a.SeasonId == seasonId)
            .SumAsync(a => a.LabourCost, ct);
}
