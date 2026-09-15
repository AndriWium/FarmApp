using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class InputPurchaseLineRepository(FarmAppDbContext db) : IInputPurchaseLineRepository
{
    public Task<List<TResult>> GetByPurchaseIdAsync<TResult>(
        int purchaseId, Expression<Func<InputPurchaseLine, TResult>> selector, CancellationToken ct)
        => db.InputPurchaseLines.AsNoTracking()
            .Where(x => x.InputPurchaseId == purchaseId)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<InputPurchaseLine> lines, CancellationToken ct)
        => await db.InputPurchaseLines.AddRangeAsync(lines, ct);
}
