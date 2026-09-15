using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Activity, TResult>> selector, CancellationToken ct);

    /// <summary>Optional seasonId filter - GET /api/v1/activities?seasonId= (task brief).</summary>
    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Activity, TResult>> selector, int? seasonId, CancellationToken ct);

    Task AddAsync(Activity activity, CancellationToken ct);
}
