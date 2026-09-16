using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class ExpenseCategoryRepository(FarmAppDbContext db) : IExpenseCategoryRepository
{
    public Task<ExpenseCategory?> GetByIdAsync(int id, CancellationToken ct)
        => db.ExpenseCategories.FirstOrDefaultAsync(x => x.ExpenseCategoryId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<ExpenseCategory, TResult>> selector, CancellationToken ct)
        => db.ExpenseCategories.AsNoTracking()
            .Where(x => x.ExpenseCategoryId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<ExpenseCategory, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.ExpenseCategories.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        => db.ExpenseCategories.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.ExpenseCategoryId != excludeId), ct);

    public async Task AddAsync(ExpenseCategory category, CancellationToken ct)
        => await db.ExpenseCategories.AddAsync(category, ct);
}
