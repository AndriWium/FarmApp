using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class ExpenseRepository(FarmAppDbContext db) : IExpenseRepository
{
    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Expense, TResult>> selector, CancellationToken ct)
        => db.Expenses.AsNoTracking()
            .Where(x => x.ExpenseId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Expense, TResult>> selector, DateTime? from, DateTime? to, int? categoryId, CancellationToken ct)
        => db.Expenses.AsNoTracking()
            .Where(x => (from == null || x.Date >= from) &&
                        (to == null || x.Date <= to) &&
                        (categoryId == null || x.ExpenseCategoryId == categoryId))
            .OrderByDescending(x => x.Date)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Expense expense, CancellationToken ct)
        => await db.Expenses.AddAsync(expense, ct);
}
