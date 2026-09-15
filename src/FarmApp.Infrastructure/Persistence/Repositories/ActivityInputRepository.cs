using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class ActivityInputRepository(FarmAppDbContext db) : IActivityInputRepository
{
    public Task<List<TResult>> GetByActivityIdAsync<TResult>(
        int activityId, Expression<Func<ActivityInput, TResult>> selector, CancellationToken ct)
        => db.ActivityInputs.AsNoTracking()
            .Where(x => x.ActivityId == activityId)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<ActivityInput> lines, CancellationToken ct)
        => await db.ActivityInputs.AddRangeAsync(lines, ct);

    public Task<List<ChemicalSprayRow>> GetChemicalSpraysForBlockAsync(int blockId, CancellationToken ct)
        => (from planting in db.Plantings.AsNoTracking()
            where planting.BlockId == blockId
            join season in db.Seasons.AsNoTracking() on planting.PlantingId equals season.PlantingId
            join activity in db.Activities.AsNoTracking() on season.SeasonId equals activity.SeasonId
            join ai in db.ActivityInputs.AsNoTracking() on activity.ActivityId equals ai.ActivityId
            join item in db.InputItems.AsNoTracking() on ai.InputItemId equals item.InputItemId
            where item.Category == InputItemCategory.Chemical && item.WithholdingDays != null
            select new ChemicalSprayRow(activity.ActivityId, activity.Date, item.Name, item.WithholdingDays!.Value))
            .ToListAsync(ct);
}
