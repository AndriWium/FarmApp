using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IHarvestRepository
{
    Task<Harvest?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Harvest, TResult>> selector, CancellationToken ct);

    /// <summary>Optional seasonId filter - GET /api/v1/harvests?seasonId= (matches Activity's own
    /// GET .../activities?seasonId= precedent one level up the same Season).</summary>
    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Harvest, TResult>> selector, int? seasonId, CancellationToken ct);

    Task AddAsync(Harvest harvest, CancellationToken ct);
}
