namespace FarmApp.Domain.Services;

public class SaleLineCalculator : ISaleLineCalculator
{
    public decimal ToBaseUnitQty(decimal qty, decimal? packSizeQtyInBaseUnit)
        => packSizeQtyInBaseUnit is null ? qty : qty * packSizeQtyInBaseUnit.Value;

    public decimal WeightedAverageCost(IReadOnlyList<CostedAllocation> allocations)
    {
        ArgumentNullException.ThrowIfNull(allocations);
        if (allocations.Count == 0)
            throw new ArgumentException("At least one allocation is required.", nameof(allocations));

        var totalQty = allocations.Sum(a => a.Qty);
        if (totalQty <= 0)
            throw new ArgumentOutOfRangeException(nameof(allocations), "Total allocated quantity must be positive.");

        return allocations.Sum(a => a.Qty * a.UnitCost) / totalQty;
    }
}
