namespace FarmApp.Domain.Services;

/// <summary>Pure FIFO stock-depletion decision logic (doc 11's Repository pattern / SOLID
/// sections). Fetching which batches exist is a repository concern; deciding how much comes
/// from which is domain logic - this interface is that decision, nothing else.</summary>
public interface IStockAllocationService
{
    /// <summary>Allocates quantityNeeded across availableBatchesOldestFirst, taking as much as
    /// possible from the oldest batch before spilling into the next. Throws
    /// InsufficientStockException if the batches' total available quantity is less than
    /// quantityNeeded.</summary>
    IReadOnlyList<StockAllocation> AllocateFifo(
        IReadOnlyList<BatchAvailability> availableBatchesOldestFirst, decimal quantityNeeded);
}
