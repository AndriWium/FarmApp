namespace FarmApp.Domain.Entities;

public class AppUser
{
    public int AppUserId { get; set; }
    public string UserName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;   // "Owner" | "Cashier" | "Worker"
    public bool IsActive { get; set; } = true;
}
