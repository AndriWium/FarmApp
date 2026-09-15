using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

/// <summary>Backs Activity's input-line sub-resource. Deliberately no top-level CRUD surface -
/// lines are only ever created as part of IActivityService.CreateActivityAsync (matches
/// IProducePurchaseLineRepository/IActivityTypeRepository's sibling precedent).</summary>
public interface IActivityInputRepository
{
    Task<List<TResult>> GetByActivityIdAsync<TResult>(
        int activityId, Expression<Func<ActivityInput, TResult>> selector, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<ActivityInput> lines, CancellationToken ct);

    /// <summary>Chemical-category spray inputs applied under any Season/Planting on the given
    /// block - the Planting -> Season -> Activity -> ActivityInput -> InputItem trace (doc 05 §5,
    /// Phase 3b task brief) that IWithholdingLockService needs to compute a block's withholding
    /// lock. Lives here (not on IPlantingRepository/ISeasonRepository/IActivityRepository)
    /// because its primary aggregate is ActivityInput rows - the other tables are joined only to
    /// filter down to one block - matching ISalePaymentRepository's "the repository owns the
    /// query whose primary entity it aggregates" precedent (see DECISIONS.md, Phase 2b).</summary>
    Task<List<ChemicalSprayRow>> GetChemicalSpraysForBlockAsync(int blockId, CancellationToken ct);
}
