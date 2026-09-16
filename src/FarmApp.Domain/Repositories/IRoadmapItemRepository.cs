using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IRoadmapItemRepository
{
    Task<RoadmapItem?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<RoadmapItem, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<RoadmapItem, TResult>> selector, CancellationToken ct);
    Task<bool> ExistsAsync(CancellationToken ct);
    Task AddAsync(RoadmapItem item, CancellationToken ct);
}
