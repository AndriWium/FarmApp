using FarmApp.Domain.Services;

namespace FarmApp.Domain.Tests.Services;

public class WithholdingLockCalculatorTests
{
    private readonly WithholdingLockCalculator _sut = new();

    [Fact]
    public void GetLockStatus_NoSprays_NotLocked()
    {
        var result = _sut.GetLockStatus([], new DateTime(2026, 6, 1));

        Assert.False(result.IsLocked);
        Assert.Null(result.LockedUntil);
        Assert.Null(result.BindingSpray);
    }

    [Fact]
    public void GetLockStatus_HarvestDateInsideWindow_IsLocked()
    {
        // Sprayed 1 June with a 7-day withholding period -> locked until 8 June.
        var sprays = new[] { new SprayWithholding(1, new DateTime(2026, 6, 1), "Copper Oxychloride", 7) };

        var result = _sut.GetLockStatus(sprays, new DateTime(2026, 6, 5));

        Assert.True(result.IsLocked);
        Assert.Equal(new DateTime(2026, 6, 8), result.LockedUntil);
        Assert.Equal(1, result.BindingSpray!.ActivityId);
    }

    [Fact]
    public void GetLockStatus_HarvestDateOnLockedUntilDate_IsNotLocked()
    {
        // "may not be harvested for N days" - the day the window ends, harvesting is allowed
        // again: locked iff harvestDate < lockedUntil, not <=.
        var sprays = new[] { new SprayWithholding(1, new DateTime(2026, 6, 1), "Copper Oxychloride", 7) };

        var result = _sut.GetLockStatus(sprays, new DateTime(2026, 6, 8));

        Assert.False(result.IsLocked);
        Assert.Null(result.LockedUntil);
    }

    [Fact]
    public void GetLockStatus_HarvestDateAfterWindow_IsNotLocked()
    {
        var sprays = new[] { new SprayWithholding(1, new DateTime(2026, 6, 1), "Copper Oxychloride", 7) };

        var result = _sut.GetLockStatus(sprays, new DateTime(2026, 6, 9));

        Assert.False(result.IsLocked);
    }

    [Fact]
    public void GetLockStatus_MultipleSprays_TakesTheLatestEndingLock()
    {
        // Spray A: 1 June + 3 days -> locked until 4 June.
        // Spray B: 3 June + 14 days -> locked until 17 June (the most restrictive - doc 05 §5).
        var sprays = new[]
        {
            new SprayWithholding(1, new DateTime(2026, 6, 1), "Short-window fungicide", 3),
            new SprayWithholding(2, new DateTime(2026, 6, 3), "Long-window insecticide", 14),
        };

        var result = _sut.GetLockStatus(sprays, new DateTime(2026, 6, 10));

        Assert.True(result.IsLocked);
        Assert.Equal(new DateTime(2026, 6, 17), result.LockedUntil);
        Assert.Equal(2, result.BindingSpray!.ActivityId);
    }

    [Fact]
    public void GetLockStatus_MultipleSprays_EarlierWindowAlreadyClearedButLaterStillLocks()
    {
        // Same two sprays as above, but the harvest date is after Spray A's window closes (4
        // June) - it must still be locked because Spray B's window (until 17 June) is later.
        var sprays = new[]
        {
            new SprayWithholding(1, new DateTime(2026, 6, 1), "Short-window fungicide", 3),
            new SprayWithholding(2, new DateTime(2026, 6, 3), "Long-window insecticide", 14),
        };

        var result = _sut.GetLockStatus(sprays, new DateTime(2026, 6, 5));

        Assert.True(result.IsLocked);
        Assert.Equal(new DateTime(2026, 6, 17), result.LockedUntil);
    }

    [Fact]
    public void GetLockStatus_AllWindowsCleared_IsNotLocked()
    {
        var sprays = new[]
        {
            new SprayWithholding(1, new DateTime(2026, 6, 1), "Short-window fungicide", 3),
            new SprayWithholding(2, new DateTime(2026, 6, 3), "Long-window insecticide", 14),
        };

        var result = _sut.GetLockStatus(sprays, new DateTime(2026, 6, 20));

        Assert.False(result.IsLocked);
    }
}
