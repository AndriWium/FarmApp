using FarmApp.Api.Application.Auth;
using FarmApp.Domain.Entities;
using FarmApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController(FarmAppDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName && u.IsActive, ct);
        if (user is null) return Unauthorized();

        var hasher = new PasswordHasher<AppUser>();
        var check = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (check == PasswordVerificationResult.Failed) return Unauthorized();

        var refresh = new RefreshToken
        {
            AppUserId = user.AppUserId,
            Token = tokens.CreateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(14),
        };
        db.RefreshTokens.Add(refresh);
        await db.SaveChangesAsync(ct);

        return new TokenResponse(tokens.CreateAccessToken(user), refresh.Token);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var existing = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && t.RevokedAt == null, ct);
        if (existing is null || existing.ExpiresAt < DateTime.UtcNow) return Unauthorized();

        var user = await db.Users.FirstAsync(u => u.AppUserId == existing.AppUserId, ct);
        existing.RevokedAt = DateTime.UtcNow;

        var newRefresh = new RefreshToken
        {
            AppUserId = user.AppUserId,
            Token = tokens.CreateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(14),
        };
        db.RefreshTokens.Add(newRefresh);
        await db.SaveChangesAsync(ct);

        return new TokenResponse(tokens.CreateAccessToken(user), newRefresh.Token);
    }
}
