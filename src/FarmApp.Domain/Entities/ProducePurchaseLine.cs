namespace FarmApp.Domain.Entities;

/// <summary>One product/grade/qty/cost line of a ProducePurchase. Each line is the reason a
/// StockBatch (Source: Purchase) exists - StockBatch.PurchaseLineId points back to
/// ProducePurchaseLineId for traceability (doc 02). A line never carries ShelfLifeDays itself
/// (not in doc 02's field list); the shelf life used to build the line's StockBatch is supplied
/// as part of the create request instead - see DECISIONS.md.</summary>
public class ProducePurchaseLine
{
    public int ProducePurchaseLineId { get; set; }
    public int ProducePurchaseId { get; set; } // plain FK column, no navigation
    public int ProductId { get; set; } // plain FK column, no navigation
    public int? GradeId { get; set; } // plain FK column, no navigation; grades don't apply to every product

    public decimal Qty { get; set; } // decimal(18,3)
    public decimal UnitCost { get; set; } // decimal(18,2)

    /// <summary>VAT shown on the supplier's slip for this line, nullable - doc 10 §2's VAT-
    /// readiness gap (should have shipped in Phase 0, closed in Phase 4b). Captured now so
    /// history is reclaimable/reportable if VAT registration comes later; not used by any
    /// calculation yet (the business isn't VAT-registered).</summary>
    public decimal? VatAmount { get; set; } // decimal(18,2)
}
