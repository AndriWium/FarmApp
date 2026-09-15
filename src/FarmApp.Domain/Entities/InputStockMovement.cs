using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

/// <summary>Append-only ledger row for input-item stock (fertiliser, seed, chemicals,
/// packaging) - never updated or deleted, only inserted. On-hand per item is always SUM(Qty)
/// over these rows, same "derive, never store a mutable balance column" discipline as the
/// produce-side StockMovement (doc 02), but simpler: one row per movement against InputItemId
/// directly, no StockBatch/grade/FIFO concept (input items aren't lot-tracked - Phase 3a task
/// brief). Implements IPeriodLocked, same precedent as StockMovement/Sale - a movement can't be
/// posted into a closed accounting month (see DECISIONS.md).</summary>
public class InputStockMovement : IPeriodLocked
{
    public int InputStockMovementId { get; set; }
    public int InputItemId { get; set; } // plain FK column, no navigation
    public DateTime Date { get; set; }
    public InputStockMovementType Type { get; set; }

    /// <summary>Signed: positive for PurchaseIn, negative for Consumption; Adjustment can go
    /// either way (stock-take correction). The sign is a business rule the service layer
    /// enforces when it constructs a movement - callers never pass a signed value in directly.</summary>
    public decimal Qty { get; set; } // decimal(18,3)

    /// <summary>Set only on PurchaseIn rows - the cost recorded at that purchase. Denormalized
    /// onto the movement itself rather than requiring a join back to InputPurchaseLine via
    /// RefId, so GetWeightedAverageCostAsync stays a single-table aggregate query (see
    /// DECISIONS.md for the join-vs-denormalize tradeoff). Null for Consumption/Adjustment rows.</summary>
    public decimal? UnitCost { get; set; } // decimal(18,2)

    /// <summary>Loose polymorphic pointer back to whatever header record caused this movement
    /// (e.g. "InputPurchaseLine"/123 for PurchaseIn, "Activity"/45 for Consumption), for
    /// traceability - matches StockMovement's RefTable/RefId convention.</summary>
    public string? RefTable { get; set; }
    public int? RefId { get; set; }

    public string? Reason { get; set; }

    DateTime IPeriodLocked.BusinessDate => Date;
}
