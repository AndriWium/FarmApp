namespace FarmApp.Domain.Services;

/// <summary>A season's actual accumulated costs/yield at the moment of close (doc 09) - InputCost
/// is Σ(ActivityInput.UnitCost x Qty), LabourCost is Σ Activity.LabourCost, both across every
/// Activity belonging to the season; TotalKgHarvested is Σ HarvestLine.QtyKg across every Harvest
/// belonging to the season. OverheadAllocated is always 0 for now (doc 09/11: "start by not
/// allocating") - carried as an explicit parameter rather than hard-coded so the calculator
/// doesn't need to change shape when that later refinement lands.</summary>
public record SeasonAccumulatedCosts(decimal InputCost, decimal LabourCost, decimal OverheadAllocated, decimal TotalKgHarvested);

/// <summary>One harvest-sourced StockBatch's qty and the estimate it was snapshotted at (doc 09).
/// A season's estimate can change mid-season (Season.EstimatedCostPerKg is editable while open),
/// so different batches from the same season can legitimately carry different EstimatedUnitCost
/// values - the true-up is computed per batch rather than against one blended season-wide
/// estimate, to honour that exactly.</summary>
public record HarvestBatchCost(decimal QtyKg, decimal EstimatedUnitCost);

/// <summary>Pure arithmetic for doc 09's costing rule: cost own-grown produce at an estimate
/// during the season, then true-up to the actual figure once the season closes. Zero
/// dependencies - no EF, no database, no I/O (doc 11) - same shape as
/// SaleLineCalculator/StockAllocationService.</summary>
public interface ISeasonCostCalculator
{
    /// <summary>ExpectedTotalCost ÷ ExpectedYieldKg, rounded to 2dp (money) - what
    /// Season.EstimatedCostPerKg is server-computed from on every create/update (never trusted
    /// from the client directly). Returns null when either input hasn't been supplied yet or
    /// ExpectedYieldKg isn't positive (doc 09's "÷0 — undefined" case) - a season legitimately
    /// starts with no estimate at all before a human types one in.</summary>
    decimal? CalculateEstimatedCostPerKg(decimal? expectedTotalCost, decimal? expectedYieldKg);

    /// <summary>(InputCost + LabourCost + OverheadAllocated) ÷ TotalKgHarvested, rounded to 2dp -
    /// the real, actual cost per kg, only knowable once the season closes (doc 09). Returns 0
    /// when TotalKgHarvested isn't positive (nothing harvested yet - guards the divide-by-zero
    /// case sensibly rather than throwing, since a season can genuinely close having harvested
    /// nothing, e.g. a failed crop).</summary>
    decimal CalculateCostPerKg(SeasonAccumulatedCosts costs);

    /// <summary>Σ(actualCostPerKg - batch.EstimatedUnitCost) x batch.QtyKg across every
    /// harvest-sourced batch the season produced - the one labelled "costing true-up" adjustment
    /// doc 09 describes, computed per batch so a mid-season estimate change is honoured exactly.
    /// Negative means sales were overcosted during the season (COS overstated, a credit);
    /// positive means undercosted. Rounded to 2dp (money).</summary>
    decimal CalculateTrueUpAmount(decimal actualCostPerKg, IReadOnlyList<HarvestBatchCost> harvestBatches);
}

public class SeasonCostCalculator : ISeasonCostCalculator
{
    public decimal? CalculateEstimatedCostPerKg(decimal? expectedTotalCost, decimal? expectedYieldKg)
    {
        if (expectedTotalCost is null || expectedYieldKg is null || expectedYieldKg.Value <= 0)
            return null;

        return Math.Round(expectedTotalCost.Value / expectedYieldKg.Value, 2, MidpointRounding.AwayFromZero);
    }

    public decimal CalculateCostPerKg(SeasonAccumulatedCosts costs)
    {
        ArgumentNullException.ThrowIfNull(costs);
        if (costs.TotalKgHarvested <= 0) return 0m;

        var totalCost = costs.InputCost + costs.LabourCost + costs.OverheadAllocated;
        return Math.Round(totalCost / costs.TotalKgHarvested, 2, MidpointRounding.AwayFromZero);
    }

    public decimal CalculateTrueUpAmount(decimal actualCostPerKg, IReadOnlyList<HarvestBatchCost> harvestBatches)
    {
        ArgumentNullException.ThrowIfNull(harvestBatches);
        var total = harvestBatches.Sum(b => (actualCostPerKg - b.EstimatedUnitCost) * b.QtyKg);
        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
