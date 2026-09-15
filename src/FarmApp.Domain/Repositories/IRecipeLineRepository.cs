using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

/// <summary>Backs Product's recipe sub-resource. Deliberately no top-level CRUD surface -
/// used only by IProductService to replace a product's whole recipe atomically.</summary>
public interface IRecipeLineRepository
{
    /// <summary>Tracked - used to delete the current set before inserting the replacement.</summary>
    Task<List<RecipeLine>> GetByProductIdAsync(int productId, CancellationToken ct);

    Task<List<TResult>> GetByProductIdAsync<TResult>(
        int productId, Expression<Func<RecipeLine, TResult>> selector, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<RecipeLine> lines, CancellationToken ct);
    void RemoveRange(IEnumerable<RecipeLine> lines);
}
