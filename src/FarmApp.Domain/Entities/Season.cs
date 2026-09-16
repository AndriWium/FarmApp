namespace FarmApp.Domain.Entities;

/// <summary>Open while the season is being farmed and costed at an estimate; Closed once
/// ISeasonCostingService.ConfirmCloseAsync has posted the true-up (doc 09) - same Open/Closed
/// shape as AccountingPeriodStatus. A closed season rejects further harvests and can't be
/// re-closed (idempotency guard) - see SeasonCostingService/HarvestService.</summary>
public enum SeasonStatus
{
    Open,
    Closed,
}

/// <summary>For perennials: one Season per production year. For annuals: Planting roughly equals
/// Season, but the table is kept anyway for uniform costing (doc 02) - every Activity hangs off
/// a Season, never directly off a Planting, so an annual crop still needs one Season row.
/// StartDate/EndDate are both required (doc 02's field list carries no "EndDate NULL" marker,
/// unlike Planting) - a season is a defined production-year window decided up front (e.g. "2026
/// season, 1 Jul - 30 Jun"), not an open-ended thing that "ends" later (see DECISIONS.md).</summary>
public class Season
{
    public int SeasonId { get; set; }
    public int PlantingId { get; set; } // plain FK column, no navigation
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public SeasonStatus Status { get; set; } = SeasonStatus.Open;

    // doc 09 costing-estimate fields, editable while Status == Open (SeasonService rejects
    // UpdateAsync once Closed). ExpectedTotalCost/ExpectedYieldKg are the two numbers a human
    // types in (can start crude - last season's number, or gut feel); EstimatedCostPerKg is
    // always server-computed from those two (ISeasonCostCalculator.CalculateEstimatedCostPerKg),
    // never trusted from the client directly - it's what HarvestService snapshots into each new
    // StockBatch.UnitCost while the season is open (doc 09: "snapshot the estimate").
    public decimal? ExpectedTotalCost { get; set; } // decimal(18,2)
    public decimal? ExpectedYieldKg { get; set; } // decimal(18,3)
    public decimal? EstimatedCostPerKg { get; set; } // decimal(18,2)
}
