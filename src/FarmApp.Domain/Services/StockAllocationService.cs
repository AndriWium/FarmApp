namespace FarmApp.Domain.Services;

/// <summary>Pure, unit-testable FIFO depletion. Zero dependencies - no EF, no database, no I/O
/// (doc 11). If this class ever wants FarmAppDbContext or an EF Core using, the design is
/// wrong; that concern belongs in the repository/service layer instead.</summary>
public class StockAllocationService : IStockAllocationService
{
    public IReadOnlyList<StockAllocation> AllocateFifo(
        IReadOnlyList<BatchAvailability> availableBatchesOldestFirst, decimal quantityNeeded)
    {
        ArgumentNullException.ThrowIfNull(availableBatchesOldestFirst);
        if (quantityNeeded <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantityNeeded), "Quantity needed must be positive.");

        var totalAvailable = availableBatchesOldestFirst.Sum(b => b.QtyAvailable);
        if (totalAvailable < quantityNeeded)
            throw new InsufficientStockException(quantityNeeded, totalAvailable);

        var allocations = new List<StockAllocation>();
        var remaining = quantityNeeded;

        foreach (var batch in availableBatchesOldestFirst)
        {
            if (remaining <= 0) break;
            if (batch.QtyAvailable <= 0) continue;

            var take = Math.Min(batch.QtyAvailable, remaining);
            allocations.Add(new StockAllocation(batch.BatchId, take));
            remaining -= take;
        }

        return allocations;
    }
}
