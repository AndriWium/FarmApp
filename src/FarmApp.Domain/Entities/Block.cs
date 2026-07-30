namespace FarmApp.Domain.Entities;

public class Block
{
    public int BlockId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal AreaHectare { get; set; } // 1 ha = 10,000 m² - decimal(18,3)
    public string Note { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
