using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class PackSizeRepository(FarmAppDbContext db) : IPackSizeRepository
{
    public Task<PackSize?> GetByIdAsync(int id, CancellationToken ct)
        => db.PackSizes.FirstOrDefaultAsync(x => x.PackSizeId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<PackSize, TResult>> selector, CancellationToken ct)
        => db.PackSizes.AsNoTracking()
            .Where(x => x.PackSizeId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<PackSize, TResult>> selector, CancellationToken ct)
        => db.PackSizes.AsNoTracking()
            .OrderBy(x => x.ProductId).ThenBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(int productId, string name, int? excludeId, CancellationToken ct)
        => db.PackSizes.AsNoTracking()
            .AnyAsync(x => x.ProductId == productId && x.Name == name && (excludeId == null || x.PackSizeId != excludeId), ct);

    public async Task AddAsync(PackSize packSize, CancellationToken ct)
        => await db.PackSizes.AddAsync(packSize, ct);

    public void Remove(PackSize packSize)
        => db.PackSizes.Remove(packSize);
}
