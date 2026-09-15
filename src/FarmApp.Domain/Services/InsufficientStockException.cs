namespace FarmApp.Domain.Services;

/// <summary>Thrown by IStockAllocationService.AllocateFifo when the total available quantity
/// across all supplied batches is less than the quantity requested. The caller must not
/// silently under-allocate - a shortfall is always surfaced, never swallowed.</summary>
public class InsufficientStockException(decimal requested, decimal available)
    : Exception($"Insufficient stock: requested {requested}, only {available} available.")
{
    public decimal Requested { get; } = requested;
    public decimal Available { get; } = available;
}
