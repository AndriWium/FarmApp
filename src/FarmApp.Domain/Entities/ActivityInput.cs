namespace FarmApp.Domain.Entities;

/// <summary>One input-item/qty line of an Activity - consumes input stock (a Consumption
/// InputStockMovement per line, RefTable "Activity"/RefId this Activity's id) and snapshots
/// UnitCost from IInputStockMovementRepository.GetWeightedAverageCostAsync at the moment of
/// consumption (doc 02: "snapshot the cost at time of use") - never recomputed historically,
/// same discipline as SaleLine.CostAtSale.</summary>
public class ActivityInput
{
    public int ActivityInputId { get; set; }
    public int ActivityId { get; set; } // plain FK column, no navigation
    public int InputItemId { get; set; } // plain FK column, no navigation

    public decimal Qty { get; set; } // decimal(18,3)
    public decimal UnitCost { get; set; } // decimal(18,2) - snapshot, not looked up again later
}
