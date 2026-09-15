namespace FarmApp.Domain.Entities;

/// <summary>Marker for transactional, dated entities whose writes must be rejected once their
/// BusinessDate falls in a Closed AccountingPeriod (AI Guide/10-go-live-controls.md §1), enforced
/// by PeriodLockInterceptor. No entity implements this yet — Sale and StockMovement (Phase 1/2)
/// will be the first — Grade/Block/Crop are undated master data and deliberately don't.</summary>
public interface IPeriodLocked
{
    DateTime BusinessDate { get; }
}
