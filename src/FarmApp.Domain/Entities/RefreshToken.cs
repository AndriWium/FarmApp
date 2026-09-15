namespace FarmApp.Domain.Entities;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public int AppUserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
