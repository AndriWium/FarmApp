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

    /// <summary>Σ LabourCost across every Activity belonging to the season - the direct-labour
    /// half of ISeasonCostingService's actual season cost (doc 09; the other half,
    /// input cost, is IActivityInputRepository's, since ActivityInput is its own aggregate).
    /// Returns 0 for a season with no activities yet, never null.</summary>
    Task<decimal> GetTotalLabourCostForSeasonAsync(int seasonId, CancellationToken ct);
}
