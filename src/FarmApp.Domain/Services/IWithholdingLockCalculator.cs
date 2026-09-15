namespace FarmApp.Domain.Services;

/// <summary>Pure date arithmetic for the chemical withholding-period lock (doc 05 §5). Zero
/// dependencies - no EF, no database, no I/O (doc 11) - same shape as
/// IStockAllocationService/ISaleLineCalculator. Fetching which sprays happened on a block is a
/// repository concern (IWithholdingLockService, Api/Application); deciding whether a harvest
/// date falls inside the resulting lock window is this domain logic, nothing else.</summary>
public interface IWithholdingLockCalculator
{
    /// <summary>Computes whether harvestDate falls inside any spray's withholding window. A block
    /// is locked when harvestDate is earlier than the latest LockedUntil across every spray in
    /// sprays (the most restrictive one) - this is equivalent to "locked by at least one spray",
    /// since any spray's LockedUntil is at most the latest one. sprays need not be sorted; may be
    /// empty (never sprayed, or no chemical had a withholding period - not locked).</summary>
    WithholdingLockResult GetLockStatus(IReadOnlyList<SprayWithholding> sprays, DateTime harvestDate);
}
