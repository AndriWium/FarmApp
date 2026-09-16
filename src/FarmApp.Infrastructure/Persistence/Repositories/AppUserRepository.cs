using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class AppUserRepository(FarmAppDbContext db) : IAppUserRepository
{
    public Task<AppUser?> GetByIdAsync(int id, CancellationToken ct)
        => db.Users.FirstOrDefaultAsync(x => x.AppUserId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<AppUser, TResult>> selector, CancellationToken ct)
        => db.Users.AsNoTracking()
            .Where(x => x.AppUserId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<AppUser, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.Users.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.UserName)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByUserNameAsync(string userName, int? excludeId, CancellationToken ct)
        => db.Users.AsNoTracking()
            .AnyAsync(x => x.UserName == userName && (excludeId == null || x.AppUserId != excludeId), ct);

    public async Task AddAsync(AppUser user, CancellationToken ct)
        => await db.Users.AddAsync(user, ct);

    public async Task RevokeRefreshTokensAsync(int appUserId, CancellationToken ct)
    {
        var tokens = await db.RefreshTokens
            .Where(t => t.AppUserId == appUserId && t.RevokedAt == null)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var token in tokens)
            token.RevokedAt = now;
    }
}
