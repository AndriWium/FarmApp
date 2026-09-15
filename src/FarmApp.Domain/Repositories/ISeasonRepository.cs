using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ISeasonRepository
{
    Task<Season?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Season, TResult>> selector, CancellationToken ct);

    /// <summary>Optional plantingId filter - GET /api/v1/seasons?plantingId=.</summary>
    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Season, TResult>> selector, int? plantingId, CancellationToken ct);

    Task AddAsync(Season season, CancellationToken ct);
}
