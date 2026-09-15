namespace FarmApp.Domain.Services;

public class WithholdingLockCalculator : IWithholdingLockCalculator
{
    public WithholdingLockResult GetLockStatus(IReadOnlyList<SprayWithholding> sprays, DateTime harvestDate)
    {
        ArgumentNullException.ThrowIfNull(sprays);

        if (sprays.Count == 0)
            return new WithholdingLockResult(false, null, null);

        // The most restrictive spray is whichever ends latest - if harvestDate clears that one,
        // it necessarily clears every other (each other LockedUntil is <= this one's).
        var binding = sprays.OrderByDescending(s => s.LockedUntil).First();

        return harvestDate < binding.LockedUntil
            ? new WithholdingLockResult(true, binding.LockedUntil, binding)
            : new WithholdingLockResult(false, null, null);
    }
}
