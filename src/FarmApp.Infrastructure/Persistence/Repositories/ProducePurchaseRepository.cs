using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class ProducePurchaseRepository(FarmAppDbContext db) : IProducePurchaseRepository
{
    public Task<ProducePurchase?> GetByIdAsync(int id, CancellationToken ct)
        => db.ProducePurchases.FirstOrDefaultAsync(x => x.ProducePurchaseId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<ProducePurchase, TResult>> selector, CancellationToken ct)
        => db.ProducePurchases.AsNoTracking()
            .Where(x => x.ProducePurchaseId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<ProducePurchase, TResult>> selector, CancellationToken ct)
        => db.ProducePurchases.AsNoTracking()
            .OrderByDescending(x => x.Date)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(ProducePurchase purchase, CancellationToken ct)
        => await db.ProducePurchases.AddAsync(purchase, ct);
}
