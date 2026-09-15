namespace FarmApp.Domain.Entities;

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
}
