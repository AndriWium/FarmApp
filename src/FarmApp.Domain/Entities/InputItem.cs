using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

public class InputItem
{
    public int InputItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public InputItemCategory Category { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal ReorderLevel { get; set; } // decimal(18,3)
    public int? WithholdingDays { get; set; } // chemicals only
    public string? ActiveIngredient { get; set; } // chemicals only
    public bool IsActive { get; set; } = true;
}
