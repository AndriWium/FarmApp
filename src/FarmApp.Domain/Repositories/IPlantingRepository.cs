using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IPlantingRepository
{
    Task<Planting?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Planting, TResult>> selector, CancellationToken ct);

    /// <summary>Optional blockId filter - GET /api/v1/plantings?blockId= (matches ISaleRepository's
    /// optional-filter precedent).</summary>
    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Planting, TResult>> selector, int? blockId, CancellationToken ct);

    Task AddAsync(Planting planting, CancellationToken ct);
}
