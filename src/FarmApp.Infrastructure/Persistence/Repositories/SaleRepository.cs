using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class SaleRepository(FarmAppDbContext db) : ISaleRepository
{
    public Task<Sale?> GetByIdAsync(int id, CancellationToken ct)
        => db.Sales.FirstOrDefaultAsync(x => x.SaleId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Sale, TResult>> selector, CancellationToken ct)
        => db.Sales.AsNoTracking()
            .Where(x => x.SaleId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<Sale?> GetByClientGuidAsync(Guid clientGuid, CancellationToken ct)
        => db.Sales.FirstOrDefaultAsync(x => x.ClientGuid == clientGuid, ct);

    public Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Sale, TResult>> selector, int? tillSessionId, CancellationToken ct)
        => db.Sales.AsNoTracking()
            .Where(x => tillSessionId == null || x.TillSessionId == tillSessionId)
            .OrderByDescending(x => x.DateTime)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Sale sale, CancellationToken ct)
        => await db.Sales.AddAsync(sale, ct);
}
