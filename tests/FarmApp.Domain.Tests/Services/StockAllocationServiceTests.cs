using FarmApp.Domain.Services;

namespace FarmApp.Domain.Tests.Services;

public class StockAllocationServiceTests
{
    private readonly StockAllocationService _sut = new();

    [Fact]
    public void AllocateFifo_SpillingIntoNextBatch_TakesOldestFirstThenSpillsOver()
    {
        // Oldest batch (1) only has 3kg, so a 5kg request must take all of batch 1 then
        // spill the remaining 2kg into the next-oldest batch (2), leaving batch 3 untouched.
        var batches = new[]
        {
            new BatchAvailability(BatchId: 1, Date: new DateTime(2026, 1, 1), QtyAvailable: 3m),
            new BatchAvailability(BatchId: 2, Date: new DateTime(2026, 1, 5), QtyAvailable: 10m),
            new BatchAvailability(BatchId: 3, Date: new DateTime(2026, 1, 10), QtyAvailable: 10m),
        };

        var result = _sut.AllocateFifo(batches, 5m);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].BatchId);
        Assert.Equal(3m, result[0].QtyToTake);
        Assert.Equal(2, result[1].BatchId);
        Assert.Equal(2m, result[1].QtyToTake);
    }

    [Fact]
    public void AllocateFifo_ExactlyTotalAvailableAcrossBatches_LeavesNothingOver()
    {
        var batches = new[]
        {
            new BatchAvailability(1, new DateTime(2026, 1, 1), 4m),
            new BatchAvailability(2, new DateTime(2026, 1, 2), 6m),
        };

        var result = _sut.AllocateFifo(batches, 10m);

        Assert.Equal(2, result.Count);
        Assert.Equal(4m, result[0].QtyToTake);
        Assert.Equal(6m, result[1].QtyToTake);
        Assert.Equal(10m, result.Sum(a => a.QtyToTake));
    }

    [Fact]
    public void AllocateFifo_RequestExceedsTotalAvailable_ThrowsInsufficientStock()
    {
        var batches = new[]
        {
            new BatchAvailability(1, new DateTime(2026, 1, 1), 4m),
            new BatchAvailability(2, new DateTime(2026, 1, 2), 6m),
        };

        var ex = Assert.Throws<InsufficientStockException>(() => _sut.AllocateFifo(batches, 10.001m));

        Assert.Equal(10.001m, ex.Requested);
        Assert.Equal(10m, ex.Available);
    }

    [Fact]
    public void AllocateFifo_SingleBatchCoversRequest_DoesNotSplitUnnecessarily()
    {
        var batches = new[]
        {
            new BatchAvailability(1, new DateTime(2026, 1, 1), 3m),
            new BatchAvailability(2, new DateTime(2026, 1, 2), 50m),
        };

        var result = _sut.AllocateFifo(batches, 3m);

        var allocation = Assert.Single(result);
        Assert.Equal(1, allocation.BatchId);
        Assert.Equal(3m, allocation.QtyToTake);
    }

    [Fact]
    public void AllocateFifo_ZeroOrNegativeQuantity_Throws()
    {
        var batches = new[] { new BatchAvailability(1, new DateTime(2026, 1, 1), 10m) };

        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.AllocateFifo(batches, 0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.AllocateFifo(batches, -1m));
    }

    [Fact]
    public void AllocateFifo_SkipsExhaustedBatchesInTheMiddle()
    {
        // Batch 2 is already at zero (e.g. fully depleted by an earlier movement) — the
        // allocator must skip it rather than produce a zero-qty allocation row for it.
        var batches = new[]
        {
            new BatchAvailability(1, new DateTime(2026, 1, 1), 2m),
            new BatchAvailability(2, new DateTime(2026, 1, 2), 0m),
            new BatchAvailability(3, new DateTime(2026, 1, 3), 5m),
        };

        var result = _sut.AllocateFifo(batches, 4m);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].BatchId);
        Assert.Equal(2m, result[0].QtyToTake);
        Assert.Equal(3, result[1].BatchId);
        Assert.Equal(2m, result[1].QtyToTake);
    }
}
