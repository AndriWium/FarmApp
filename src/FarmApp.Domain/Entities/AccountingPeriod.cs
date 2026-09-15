namespace FarmApp.Domain.Entities;

public enum AccountingPeriodStatus
{
    Open,
    Closed,
}

/// <summary>Month-end lock (AI Guide/10-go-live-controls.md §1). Scaffolding only in Phase 0a:
/// the close-checklist screen and the "late paperwork" DocumentDate flow land in Phase 4 — this
/// table plus PeriodLockInterceptor exist now so no write path needs retrofitting later.</summary>
public class AccountingPeriod
{
    public int AccountingPeriodId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public AccountingPeriodStatus Status { get; set; } = AccountingPeriodStatus.Open;
    public DateTime? ClosedAt { get; set; }
    public string? ClosedBy { get; set; }
    public DateTime? ReopenedAt { get; set; }
    public string? ReopenReason { get; set; }
}
