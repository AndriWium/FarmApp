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
}
