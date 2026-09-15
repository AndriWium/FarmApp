namespace FarmApp.Api.Application.WithholdingLocks;

/// <summary>Same shape used both by the read-only proactive-warning endpoint
/// (GET /api/v1/blocks/{id}/withholding-status) and internally by IHarvestService's own
/// enforcement (doc 05 §5: "warn... when a harvest is entered inside the window" - a genuine
/// warn-first UX, not just a server-side gate discovered on submit). LockedUntil/Reason are null
/// when not locked.</summary>
public record WithholdingStatusDto(bool IsLocked, DateTime? LockedUntil, string? Reason);
