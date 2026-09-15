using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Location, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Location, TResult>> selector, bool includeInactive, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct);
    Task AddAsync(Location location, CancellationToken ct);
}
