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

    /// <summary>Σ QtyKg across every HarvestLine whose Harvest belongs to the season - the
    /// TotalKgHarvested half of ISeasonCostingService's actual season yield (doc 09). Returns 0
    /// for a season with no harvests yet, never null.</summary>
    Task<decimal> GetTotalKgHarvestedForSeasonAsync(int seasonId, CancellationToken ct);
}
