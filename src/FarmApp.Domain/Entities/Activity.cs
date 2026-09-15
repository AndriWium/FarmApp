namespace FarmApp.Domain.Entities;

/// <summary>The activity-log diary entry (doc 01): SeasonId/ActivityTypeId are cross-repo-
/// validated FKs. Implements IPeriodLocked - same precedent as StockMovement/Sale (dated,
/// transactional, doc 10 §1's "every transactional insert validates its date against the
/// period status") - not explicitly called out in the Phase 3a task brief but consistent with
/// every other dated transactional entity in the codebase (see DECISIONS.md).</summary>
public class Activity : IPeriodLocked
{
    public int ActivityId { get; set; }
    public int SeasonId { get; set; } // plain FK column, no navigation
    public int ActivityTypeId { get; set; } // plain FK column, no navigation
    public DateTime Date { get; set; }
    public decimal LabourHours { get; set; } // decimal(18,3) - a quantity
    public decimal LabourCost { get; set; } // decimal(18,2) - money
    public string? Notes { get; set; }

    DateTime IPeriodLocked.BusinessDate => Date;
}
