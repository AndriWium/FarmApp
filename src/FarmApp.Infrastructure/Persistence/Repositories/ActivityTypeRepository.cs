using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class ActivityTypeRepository(FarmAppDbContext db) : IActivityTypeRepository
{
    public Task<ActivityType?> GetByIdAsync(int id, CancellationToken ct)
        => db.ActivityTypes.FirstOrDefaultAsync(x => x.ActivityTypeId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<ActivityType, TResult>> selector, CancellationToken ct)
        => db.ActivityTypes.AsNoTracking()
            .Where(x => x.ActivityTypeId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<ActivityType, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.ActivityTypes.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        => db.ActivityTypes.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.ActivityTypeId != excludeId), ct);

    public async Task AddAsync(ActivityType activityType, CancellationToken ct)
        => await db.ActivityTypes.AddAsync(activityType, ct);
}
