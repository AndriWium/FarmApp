namespace FarmApp.Domain.Entities;

public class PriceList
{
    public int PriceListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
