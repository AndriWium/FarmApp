using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class RecipeLineRepository(FarmAppDbContext db) : IRecipeLineRepository
{
    public Task<List<RecipeLine>> GetByProductIdAsync(int productId, CancellationToken ct)
        => db.RecipeLines.Where(x => x.ProductId == productId).ToListAsync(ct);

    public Task<List<TResult>> GetByProductIdAsync<TResult>(
        int productId, Expression<Func<RecipeLine, TResult>> selector, CancellationToken ct)
        => db.RecipeLines.AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<RecipeLine> lines, CancellationToken ct)
        => await db.RecipeLines.AddRangeAsync(lines, ct);

    public void RemoveRange(IEnumerable<RecipeLine> lines)
        => db.RecipeLines.RemoveRange(lines);
}
