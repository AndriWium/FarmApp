using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

/// <summary>Backs InputPurchase's line sub-resource. Deliberately no top-level CRUD surface -
/// lines are only ever created as part of IInputPurchaseService.CreatePurchaseAsync (matches
/// IProducePurchaseLineRepository's precedent).</summary>
public interface IInputPurchaseLineRepository
{
    Task<List<TResult>> GetByPurchaseIdAsync<TResult>(
        int purchaseId, Expression<Func<InputPurchaseLine, TResult>> selector, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<InputPurchaseLine> lines, CancellationToken ct);
}
