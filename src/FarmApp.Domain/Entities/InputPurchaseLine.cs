namespace FarmApp.Domain.Entities;

/// <summary>One input-item/qty/cost line of an InputPurchase. Each line is the reason a
/// PurchaseIn InputStockMovement exists (InputStockMovement.RefTable "InputPurchaseLine",
/// RefId this line's id) - unlike ProducePurchaseLine, a line here creates no StockBatch: input
/// items aren't lot-tracked, on-hand is a plain SUM(Qty) over the movement ledger (doc 02).</summary>
public class InputPurchaseLine
{
    public int InputPurchaseLineId { get; set; }
    public int InputPurchaseId { get; set; } // plain FK column, no navigation
    public int InputItemId { get; set; } // plain FK column, no navigation

    public decimal Qty { get; set; } // decimal(18,3)
    public decimal UnitCost { get; set; } // decimal(18,2)

    /// <summary>VAT shown on the supplier's slip for this line, nullable - doc 10 §2's VAT-
    /// readiness gap (should have shipped in Phase 0, closed in Phase 4b). Captured now so
    /// history is reclaimable/reportable if VAT registration comes later; not used by any
    /// calculation yet (the business isn't VAT-registered).</summary>
    public decimal? VatAmount { get; set; } // decimal(18,2)
}
