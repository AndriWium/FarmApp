namespace FarmApp.Domain.Services;

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
}

public class SeasonCostCalculator : ISeasonCostCalculator
{
    public decimal? CalculateEstimatedCostPerKg(decimal? expectedTotalCost, decimal? expectedYieldKg)
    {
        if (expectedTotalCost is null || expectedYieldKg is null || expectedYieldKg.Value <= 0)
            return null;

        return Math.Round(expectedTotalCost.Value / expectedYieldKg.Value, 2, MidpointRounding.AwayFromZero);
    }
}
