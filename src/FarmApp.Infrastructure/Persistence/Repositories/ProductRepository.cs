using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class ProductRepository(FarmAppDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken ct)
        => db.Products.FirstOrDefaultAsync(x => x.ProductId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Product, TResult>> selector, CancellationToken ct)
        => db.Products.AsNoTracking()
            .Where(x => x.ProductId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Product, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.Products.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        => db.Products.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.ProductId != excludeId), ct);

    public async Task AddAsync(Product product, CancellationToken ct)
        => await db.Products.AddAsync(product, ct);
}
