using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

/// <summary>Backs Harvest's line sub-resource. Deliberately no top-level CRUD surface - lines are
/// only ever created as part of IHarvestService.CreateHarvestAsync (matches
/// IProducePurchaseLineRepository/IActivityInputRepository's sibling precedent).</summary>
public interface IHarvestLineRepository
{
    Task<List<TResult>> GetByHarvestIdAsync<TResult>(
        int harvestId, Expression<Func<HarvestLine, TResult>> selector, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<HarvestLine> lines, CancellationToken ct);
}
