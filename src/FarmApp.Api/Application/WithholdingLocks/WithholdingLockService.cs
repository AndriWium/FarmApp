using FarmApp.Domain.Repositories;
using FarmApp.Domain.Services;

namespace FarmApp.Api.Application.WithholdingLocks;

public class WithholdingLockService(
    IBlockRepository blockRepo,
    IActivityInputRepository activityInputRepo,
    IWithholdingLockCalculator calculator) : IWithholdingLockService
{
    public async Task<WithholdingStatusDto?> GetLockStatusAsync(int blockId, DateTime harvestDate, CancellationToken ct)
    {
        var block = await blockRepo.GetByIdAsync(blockId, b => new { b.BlockId, b.Name }, ct);
        if (block is null) return null;

        var rows = await activityInputRepo.GetChemicalSpraysForBlockAsync(blockId, ct);
        var sprays = rows
            .Select(r => new SprayWithholding(r.ActivityId, r.SprayDate, r.ChemicalName, r.WithholdingDays))
            .ToList();

        var result = calculator.GetLockStatus(sprays, harvestDate);
        if (!result.IsLocked) return new WithholdingStatusDto(false, null, null);

        // Names the block, the locked-until date, and which chemical/activity caused it - enough
        // for a human to decide whether an override is warranted before they enter one (task
        // brief).
        var reason =
            $"Block '{block.Name}' is locked for harvest until {result.LockedUntil:yyyy-MM-dd} " +
            $"due to the withholding period for '{result.BindingSpray!.ChemicalName}' " +
            $"sprayed on {result.BindingSpray.SprayDate:yyyy-MM-dd} (Activity #{result.BindingSpray.ActivityId}).";

        return new WithholdingStatusDto(true, result.LockedUntil, reason);
    }
}
