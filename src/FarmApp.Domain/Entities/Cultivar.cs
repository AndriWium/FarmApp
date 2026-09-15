namespace FarmApp.Domain.Entities;

public class Cultivar
{
    public int CultivarId { get; set; }
    public int CropId { get; set; } // plain FK column, no navigation property (matches Block/Grade/Crop)
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
