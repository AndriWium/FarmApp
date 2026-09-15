using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class SupplierRepository(FarmAppDbContext db) : ISupplierRepository
{
    public Task<Supplier?> GetByIdAsync(int id, CancellationToken ct)
        => db.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Supplier, TResult>> selector, CancellationToken ct)
        => db.Suppliers.AsNoTracking()
            .Where(x => x.SupplierId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Supplier, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.Suppliers.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Supplier supplier, CancellationToken ct)
        => await db.Suppliers.AddAsync(supplier, ct);
}
