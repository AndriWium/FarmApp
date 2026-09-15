using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class PriceListRepository(FarmAppDbContext db) : IPriceListRepository
{
    public Task<PriceList?> GetByIdAsync(int id, CancellationToken ct)
        => db.PriceLists.FirstOrDefaultAsync(x => x.PriceListId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<PriceList, TResult>> selector, CancellationToken ct)
        => db.PriceLists.AsNoTracking()
            .Where(x => x.PriceListId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<PriceList, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.PriceLists.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        => db.PriceLists.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.PriceListId != excludeId), ct);

    public async Task AddAsync(PriceList priceList, CancellationToken ct)
        => await db.PriceLists.AddAsync(priceList, ct);
}
