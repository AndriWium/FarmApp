using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public CustomerType Type { get; set; }
    public int PriceListId { get; set; } // plain FK column, no navigation (matches Cultivar->Crop)
    public decimal? CreditLimit { get; set; } // decimal(18,2)
    public bool IsActive { get; set; } = true;
}
