namespace FarmApp.Domain.Services;

/// <summary>One FIFO-allocated batch's contribution to a sale line's cost: how much quantity came
/// from it and that batch's UnitCost, as fed into ISaleLineCalculator.WeightedAverageCost.</summary>
public record CostedAllocation(decimal Qty, decimal UnitCost);
