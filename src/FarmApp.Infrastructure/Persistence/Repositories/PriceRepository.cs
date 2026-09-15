using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class PriceRepository(FarmAppDbContext db) : IPriceRepository
{
    // EF Core translates `x.GradeId == gradeId` to a SQL-null-safe comparison even when both
    // sides are null (e.g. `(GradeId = @p OR (GradeId IS NULL AND @p IS NULL))`), so plain
    // equality here correctly matches a GradeId = null combination against another
    // GradeId = null row rather than silently matching nothing. Verified explicitly - see
    // verification notes in the PR/commit.
    public Task<Price?> GetActiveAsync(int priceListId, int productId, int? gradeId, int? packSizeId, CancellationToken ct)
        => db.Prices.FirstOrDefaultAsync(x =>
            x.PriceListId == priceListId && x.ProductId == productId &&
            x.GradeId == gradeId && x.PackSizeId == packSizeId && x.ValidTo == null, ct);

    public Task<TResult?> GetCurrentAsync<TResult>(int priceListId, int productId, int? gradeId, int? packSizeId,
        Expression<Func<Price, TResult>> selector, CancellationToken ct)
        => db.Prices.AsNoTracking()
            .Where(x => x.PriceListId == priceListId && x.ProductId == productId &&
                        x.GradeId == gradeId && x.PackSizeId == packSizeId && x.ValidTo == null)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetHistoryAsync<TResult>(int priceListId, int productId, int? gradeId, int? packSizeId,
        Expression<Func<Price, TResult>> selector, CancellationToken ct)
        => db.Prices.AsNoTracking()
            .Where(x => x.PriceListId == priceListId && x.ProductId == productId &&
                        x.GradeId == gradeId && x.PackSizeId == packSizeId)
            .OrderByDescending(x => x.ValidFrom)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Price price, CancellationToken ct)
        => await db.Prices.AddAsync(price, ct);
}
