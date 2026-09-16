using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IAppUserRepository
{
    Task<AppUser?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<AppUser, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<AppUser, TResult>> selector, bool includeInactive, CancellationToken ct);
    Task<bool> ExistsByUserNameAsync(string userName, int? excludeId, CancellationToken ct);
    Task AddAsync(AppUser user, CancellationToken ct);

    /// <summary>Marks every currently-unrevoked RefreshToken row belonging to this user as revoked
    /// (RevokedAt = now) - called by UserService.SetActiveAsync when deactivating a user, so a
    /// deactivated account can't silently refresh its way back into an access token (AuthController.
    /// Refresh already rejects any token with RevokedAt != null). Does not call SaveChangesAsync -
    /// the caller controls the save, matching every other repository method in this codebase.</summary>
    Task RevokeRefreshTokensAsync(int appUserId, CancellationToken ct);
}
