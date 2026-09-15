using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IRainfallLogRepository
{
    Task<RainfallLog?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<RainfallLog, TResult>> selector, CancellationToken ct);

    /// <summary>Optional [from, to] date-range filter (inclusive) - GET /api/v1/rainfall-logs?from=&amp;to=.</summary>
    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<RainfallLog, TResult>> selector, DateOnly? from, DateOnly? to, CancellationToken ct);

    Task<bool> ExistsByDateAsync(DateOnly date, int? excludeId, CancellationToken ct);
    Task AddAsync(RainfallLog rainfallLog, CancellationToken ct);
}
