namespace FarmApp.Domain.Services;

/// <summary>Pure arithmetic for the two numbers a POS sale line is easiest to get subtly wrong
/// (task brief): converting a pack-based Qty into the base-unit quantity that actually gets
/// depleted from stock, and the qty-weighted-average cost of whichever batch(es) FIFO depletion
/// touched. Zero dependencies - no EF, no database, no I/O (doc 11) - same shape as
/// IStockAllocationService.</summary>
public interface ISaleLineCalculator
{
    /// <summary>packSizeQtyInBaseUnit is the PackSize's conversion factor when the line sold a
    /// pack (SaleLine.PackSizeId set); null means qty is already in the product's base unit.</summary>
    decimal ToBaseUnitQty(decimal qty, decimal? packSizeQtyInBaseUnit);

    /// <summary>Σ(qty x unitCost) / Σqty across every batch a sale line's depletion touched - a
    /// line can legitimately span two batches at different costs, so this is never just the
    /// first batch's cost. Throws if allocations is empty or its total qty isn't positive.</summary>
    decimal WeightedAverageCost(IReadOnlyList<CostedAllocation> allocations);
}
