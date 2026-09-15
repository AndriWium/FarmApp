namespace FarmApp.Domain.Services;

/// <summary>Outcome of IWithholdingLockCalculator.GetLockStatus. BindingSpray is the single spray
/// responsible for the lock - whichever has the latest LockedUntil among every spray considered
/// (doc 05 §5: "a block could have been sprayed multiple times with different chemicals/windows
/// - take the latest-ending lock, i.e. the most restrictive"). Null LockedUntil/BindingSpray when
/// not locked.</summary>
public record WithholdingLockResult(bool IsLocked, DateTime? LockedUntil, SprayWithholding? BindingSpray);
