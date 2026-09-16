using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IExpenseRepository
{
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Expense, TResult>> selector, CancellationToken ct);

    /// <summary>All filters optional - from/to bound Date (inclusive), categoryId narrows to one
    /// ExpenseCategory. Matches Activity/Harvest's own "optional query filter" precedent.</summary>
    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Expense, TResult>> selector, DateTime? from, DateTime? to, int? categoryId, CancellationToken ct);

    Task AddAsync(Expense expense, CancellationToken ct);
}
