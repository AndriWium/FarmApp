namespace FarmApp.Domain.Entities;

public class PackSize
{
    public int PackSizeId { get; set; }
    public int ProductId { get; set; } // plain FK column, no navigation (matches Cultivar->Crop precedent)
    public string Name { get; set; } = string.Empty; // "1kg punnet", "10kg box"
    public decimal QtyInBaseUnit { get; set; } // decimal(18,3) - conversion factor to Product.BaseUnit
}
