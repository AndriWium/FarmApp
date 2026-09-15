using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class ActivityInputRepository(FarmAppDbContext db) : IActivityInputRepository
{
    public Task<List<TResult>> GetByActivityIdAsync<TResult>(
        int activityId, Expression<Func<ActivityInput, TResult>> selector, CancellationToken ct)
        => db.ActivityInputs.AsNoTracking()
            .Where(x => x.ActivityId == activityId)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<ActivityInput> lines, CancellationToken ct)
        => await db.ActivityInputs.AddRangeAsync(lines, ct);
}
