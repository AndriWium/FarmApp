using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IPackSizeRepository
{
    Task<PackSize?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<PackSize, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<PackSize, TResult>> selector, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int productId, string name, int? excludeId, CancellationToken ct);
    Task AddAsync(PackSize packSize, CancellationToken ct);
    void Remove(PackSize packSize);
}
