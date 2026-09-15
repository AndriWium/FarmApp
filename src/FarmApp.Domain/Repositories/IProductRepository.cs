using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Product, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Product, TResult>> selector, bool includeInactive, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct);
    Task AddAsync(Product product, CancellationToken ct);
}
