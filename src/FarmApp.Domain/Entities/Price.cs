namespace FarmApp.Domain.Entities;

/// <summary>Immutable price history - never updated or deleted, only superseded.
/// Write path is IPriceService.SetPriceAsync only (no Create/Update/Deactivate).</summary>
public class Price
{
    public int PriceId { get; set; }
    public int PriceListId { get; set; } // plain FK column, no navigation
    public int ProductId { get; set; } // plain FK column, no navigation
    public int? GradeId { get; set; } // grades don't apply to every product; plain FK column, no navigation (matches Block/Crop pattern)
    public int? PackSizeId { get; set; } // nullable to support "price per base unit, no specific pack" (loose kg pricing) - see DECISIONS.md
    public decimal UnitPrice { get; set; } // decimal(18,2)
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; } // null = currently active; exclusive end date once superseded
}
