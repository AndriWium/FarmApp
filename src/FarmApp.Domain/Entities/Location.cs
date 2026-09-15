namespace FarmApp.Domain.Entities;

public class Location
{
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
