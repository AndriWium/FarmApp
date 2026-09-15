using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public int? CropId { get; set; } // meaningful only when ProductType == Produce; plain FK column, no navigation (matches Block/Crop/Cultivar precedent)
    public MakeMode? MakeMode { get; set; } // meaningful only when ProductType == Prepared
    public ProductBaseUnit BaseUnit { get; set; }
    public bool IsActive { get; set; } = true;
}
