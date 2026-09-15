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
}
