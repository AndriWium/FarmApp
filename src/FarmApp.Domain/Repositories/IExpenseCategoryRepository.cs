using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IExpenseCategoryRepository
{
    Task<ExpenseCategory?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<ExpenseCategory, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<ExpenseCategory, TResult>> selector, bool includeInactive, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct);
    Task AddAsync(ExpenseCategory category, CancellationToken ct);
}
