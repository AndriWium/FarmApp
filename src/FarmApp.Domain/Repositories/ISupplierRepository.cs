using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Supplier, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Supplier, TResult>> selector, bool includeInactive, CancellationToken ct);
    Task AddAsync(Supplier supplier, CancellationToken ct);
}
