using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class InputPurchaseRepository(FarmAppDbContext db) : IInputPurchaseRepository
{
    public Task<InputPurchase?> GetByIdAsync(int id, CancellationToken ct)
        => db.InputPurchases.FirstOrDefaultAsync(x => x.InputPurchaseId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<InputPurchase, TResult>> selector, CancellationToken ct)
        => db.InputPurchases.AsNoTracking()
            .Where(x => x.InputPurchaseId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<InputPurchase, TResult>> selector, CancellationToken ct)
        => db.InputPurchases.AsNoTracking()
            .OrderByDescending(x => x.Date)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(InputPurchase purchase, CancellationToken ct)
        => await db.InputPurchases.AddAsync(purchase, ct);
}
