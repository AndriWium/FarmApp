using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IInputItemRepository
{
    Task<InputItem?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<InputItem, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<InputItem, TResult>> selector, bool includeInactive, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct);
    Task AddAsync(InputItem inputItem, CancellationToken ct);
}
