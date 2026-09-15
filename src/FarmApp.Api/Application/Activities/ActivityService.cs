using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Activities;

public class ActivityService(
    IActivityRepository repo,
    IActivityInputRepository lineRepo,
    ISeasonRepository seasonRepo,
    IActivityTypeRepository activityTypeRepo,
    IInputItemRepository inputItemRepo,
    IInputStockMovementRepository movementRepo,
    IUnitOfWork uow) : IActivityService
{
    public async Task<List<ActivityDto>> GetAllAsync(int? seasonId, CancellationToken ct)
    {
        var activities = await repo.GetAllAsync(a => a, seasonId, ct);
        var result = new List<ActivityDto>();
        foreach (var activity in activities)
            result.Add(await ToDtoAsync(activity, ct));
        return result;
    }

    public async Task<ActivityDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var activity = await repo.GetByIdAsync(id, ct);
        return activity is null ? null : await ToDtoAsync(activity, ct);
    }

    public async Task<ServiceResult<ActivityDto>> CreateActivityAsync(CreateActivityRequest request, CancellationToken ct)
    {
        // Validate every reference before writing anything - same discipline as
        // ProducePurchaseService/InputPurchaseService (task brief).
        if (await seasonRepo.GetByIdAsync(request.SeasonId, ct) is null)
            return ServiceResult<ActivityDto>.Fail(ServiceError.NotFound);

        if (await activityTypeRepo.GetByIdAsync(request.ActivityTypeId, ct) is null)
            return ServiceResult<ActivityDto>.Fail(ServiceError.NotFound);

        foreach (var line in request.Inputs)
        {
            if (await inputItemRepo.GetByIdAsync(line.InputItemId, ct) is null)
                return ServiceResult<ActivityDto>.Fail(ServiceError.NotFound);
        }

        // Everything above is deterministic from the request alone; only input-stock
        // availability can still legitimately fail from here - so the real transaction starts
        // now. Same genuine-EF-Core-transaction pattern as SaleService.CreateSaleAsync (task
        // brief: "wrap the whole operation in a genuine transaction... don't fall back to the
        // older two-phase-save stopgap"), because this is a multi-line depleting operation of
        // the same shape.
        await uow.BeginTransactionAsync(ct);
        try
        {
            var activity = new Activity
            {
                SeasonId = request.SeasonId,
                ActivityTypeId = request.ActivityTypeId,
                Date = request.Date,
                LabourHours = request.LabourHours,
                LabourCost = request.LabourCost,
                Notes = request.Notes,
            };
            await repo.AddAsync(activity, ct);
            // Materialize ActivityId before it's used below as InputStockMovement.RefId and
            // every input line's ActivityId FK (plain FK columns, no navigation).
            await uow.SaveChangesAsync(ct);

            var inputDtos = new List<ActivityInputDto>();
            foreach (var line in request.Inputs)
            {
                // Re-reads on-hand from the database (not the change tracker) on every
                // iteration - required so two lines consuming the same InputItemId see each
                // other's depletion, not a stale snapshot (the exact bug SaleService.
                // CreateSaleAsync hit and fixed in Phase 2a - see DECISIONS.md).
                var onHand = await movementRepo.GetOnHandAsync(line.InputItemId, ct);
                if (line.Qty > onHand)
                {
                    // Insufficient stock on this line - the whole activity rolls back,
                    // including the header and any earlier line's already-flushed movements:
                    // nothing about this activity survives (task brief).
                    await uow.RollbackTransactionAsync(ct);
                    return ServiceResult<ActivityDto>.Fail(
                        ServiceError.InsufficientStock,
                        $"Input item {line.InputItemId}: requested {line.Qty}, only {onHand} on hand.");
                }

                // Snapshot the weighted-average cost at the moment of consumption - never
                // recomputed historically (doc 02, matches SaleLine.CostAtSale's discipline).
                var unitCost = await movementRepo.GetWeightedAverageCostAsync(line.InputItemId, ct);

                var activityInput = new ActivityInput
                {
                    ActivityId = activity.ActivityId,
                    InputItemId = line.InputItemId,
                    Qty = line.Qty,
                    UnitCost = unitCost,
                };
                await lineRepo.AddRangeAsync([activityInput], ct);

                var movement = new InputStockMovement
                {
                    InputItemId = line.InputItemId,
                    Date = activity.Date,
                    Type = InputStockMovementType.Consumption,
                    Qty = -line.Qty, // depletion is always negative
                    RefTable = "Activity",
                    RefId = activity.ActivityId,
                };
                await movementRepo.AddRangeAsync([movement], ct);

                // Flush this line now, before the next line's on-hand check runs (see comment above).
                await uow.SaveChangesAsync(ct);

                inputDtos.Add(new ActivityInputDto(activityInput.ActivityInputId, activityInput.InputItemId, activityInput.Qty, activityInput.UnitCost));
            }

            await uow.CommitTransactionAsync(ct);

            return ServiceResult<ActivityDto>.Ok(new ActivityDto(
                activity.ActivityId, activity.SeasonId, activity.ActivityTypeId, activity.Date,
                activity.LabourHours, activity.LabourCost, activity.Notes, inputDtos));
        }
        catch
        {
            await uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<ActivityDto> ToDtoAsync(Activity activity, CancellationToken ct)
    {
        var lines = await lineRepo.GetByActivityIdAsync(activity.ActivityId,
            l => new ActivityInputDto(l.ActivityInputId, l.InputItemId, l.Qty, l.UnitCost), ct);

        return new ActivityDto(
            activity.ActivityId, activity.SeasonId, activity.ActivityTypeId, activity.Date,
            activity.LabourHours, activity.LabourCost, activity.Notes, lines);
    }
}
