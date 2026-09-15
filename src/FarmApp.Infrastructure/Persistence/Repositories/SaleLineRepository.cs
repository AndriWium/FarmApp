using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class SaleLineRepository(FarmAppDbContext db) : ISaleLineRepository
{
    public Task<List<TResult>> GetBySaleIdAsync<TResult>(
        int saleId, Expression<Func<SaleLine, TResult>> selector, CancellationToken ct)
        => db.SaleLines.AsNoTracking()
            .Where(x => x.SaleId == saleId)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<SaleLine> lines, CancellationToken ct)
        => await db.SaleLines.AddRangeAsync(lines, ct);
}
