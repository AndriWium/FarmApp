using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

/// <summary>One receipt of sellable stock (a harvest event, a purchase line, or a production
/// run). Never carries a mutable "quantity on hand" — on-hand is always
/// SUM(StockMovement.Qty) for this batch (doc 02's central invariant). A batch doesn't get
/// deactivated: it runs out (on-hand hits zero) or gets written off via a Wastage movement, so
/// there is deliberately no IsActive here.</summary>
public class StockBatch
{
    public int StockBatchId { get; set; }
    public int ProductId { get; set; } // plain FK column, no navigation (matches Block/Crop/Cultivar precedent)
    public int? GradeId { get; set; } // grades don't apply to every product; plain FK column, no navigation

    public StockSource Source { get; set; }

    // Loose nullable pointers to the entity that created this batch. No FK constraint on any of
    // these yet: Harvest doesn't exist until Phase 3, ProducePurchaseLine is Phase 1b (right
    // after this), and ProductionBatch would need StockMovement to exist first - circular. See
    // DECISIONS.md.
    public int? HarvestId { get; set; }
    public int? PurchaseLineId { get; set; }
    public int? ProductionBatchId { get; set; }

    public DateTime Date { get; set; }
    public decimal QtyIn { get; set; } // decimal(18,3)
    public decimal UnitCost { get; set; } // decimal(18,2) - cost per base unit
    public int ShelfLifeDays { get; set; }

    /// <summary>Computed, never persisted - Date + ShelfLifeDays. Storing this as a column would
    /// let it drift from Date/ShelfLifeDays (doc 02's callout); it's recomputed on every read
    /// instead.</summary>
    public DateTime BestBeforeDate => Date.AddDays(ShelfLifeDays);
}
