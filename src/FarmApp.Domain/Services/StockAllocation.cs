namespace FarmApp.Domain.Services;

/// <summary>How much of a requested quantity to take from one batch, decided by
/// IStockAllocationService.AllocateFifo.</summary>
public record StockAllocation(int BatchId, decimal QtyToTake);
