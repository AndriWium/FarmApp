using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IInputStockMovementRepository
{
    /// <summary>SUM(Qty) for this input item - the on-hand invariant (doc 02): never a stored
    /// column, always derived from the movement ledger.</summary>
    Task<decimal> GetOnHandAsync(int inputItemId, CancellationToken ct);

    /// <summary>Weighted-average cost across every PurchaseIn movement ever recorded for this
    /// item: Sum(Qty x UnitCost) / Sum(Qty) over Type == PurchaseIn rows - the classic
    /// moving-weighted-average costing method appropriate for a non-lot-tracked item (see
    /// DECISIONS.md for why this isn't FIFO/lot-restricted). Returns 0 if the item has never
    /// been purchased (caller only calls this once on-hand > 0, which guarantees at least one
    /// PurchaseIn row exists).</summary>
    Task<decimal> GetWeightedAverageCostAsync(int inputItemId, CancellationToken ct);

    /// <summary>Inserts one or more movement rows. The caller (a use-case service) is
    /// responsible for calling IUnitOfWork.SaveChangesAsync once for the whole operation.</summary>
    Task AddRangeAsync(IEnumerable<InputStockMovement> movements, CancellationToken ct);
}
