namespace FarmApp.Domain.Entities;

/// <summary>One product/grade/pack-size line of a Sale. Qty's unit depends on PackSizeId (task
/// brief - the easiest thing in this phase to get subtly wrong): when set, Qty counts packs sold
/// and UnitPrice is quoted per pack - the base-unit quantity actually depleted from stock is
/// Qty x PackSize.QtyInBaseUnit; when null, Qty is already in Product.BaseUnit and UnitPrice is
/// per base unit. Either way the line total is simply Qty x UnitPrice - DiscountAmount, no
/// conversion needed there - the conversion only matters for stock depletion.
/// CostAtSale is the qty-weighted-average UnitCost of whichever StockBatch(es) FIFO depletion
/// actually touched for this line (a line can legitimately span two batches at different costs) -
/// snapshotted at sale time via ISaleLineCalculator.WeightedAverageCost, never recalculated
/// later (doc 02).</summary>
public class SaleLine
{
    public int SaleLineId { get; set; }
    public int SaleId { get; set; } // plain FK column, no navigation
    public int ProductId { get; set; } // plain FK column, no navigation
    public int? GradeId { get; set; } // plain FK column, no navigation
    public int? PackSizeId { get; set; } // plain FK column, no navigation

    public decimal Qty { get; set; } // decimal(18,3)
    public decimal UnitPrice { get; set; } // decimal(18,2)
    public decimal DiscountAmount { get; set; } // decimal(18,2)
    public string? DiscountReason { get; set; }
    public decimal CostAtSale { get; set; } // decimal(18,2) - weighted-average cost of the batch(es) depleted
}
