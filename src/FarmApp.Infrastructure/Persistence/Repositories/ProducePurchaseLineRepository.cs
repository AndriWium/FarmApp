using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class ProducePurchaseLineRepository(FarmAppDbContext db) : IProducePurchaseLineRepository
{
    public Task<List<TResult>> GetByPurchaseIdAsync<TResult>(
        int purchaseId, Expression<Func<ProducePurchaseLine, TResult>> selector, CancellationToken ct)
        => db.ProducePurchaseLines.AsNoTracking()
            .Where(x => x.ProducePurchaseId == purchaseId)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<ProducePurchaseLine> lines, CancellationToken ct)
        => await db.ProducePurchaseLines.AddRangeAsync(lines, ct);
}
