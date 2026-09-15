using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

/// <summary>Backs ProducePurchase's line sub-resource. Deliberately no top-level CRUD surface -
/// lines are only ever created as part of IProducePurchaseService.CreatePurchaseAsync
/// (matches IRecipeLineRepository's precedent).</summary>
public interface IProducePurchaseLineRepository
{
    Task<List<TResult>> GetByPurchaseIdAsync<TResult>(
        int purchaseId, Expression<Func<ProducePurchaseLine, TResult>> selector, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<ProducePurchaseLine> lines, CancellationToken ct);
}
