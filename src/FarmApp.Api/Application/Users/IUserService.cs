using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Users;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct);

    /// <summary>Deactivate (isActive: false) or reactivate (isActive: true) a user - the same
    /// IsActive soft-delete convention every other master-data entity in this app uses.
    /// Deactivating also revokes the user's outstanding RefreshToken rows (see
    /// IAppUserRepository.RevokeRefreshTokensAsync) so a deactivated account can't silently
    /// refresh its way back into an access token.</summary>
    Task<ServiceError> SetActiveAsync(int id, bool isActive, CancellationToken ct);
    Task<ServiceError> UpdateRoleAsync(int id, UpdateUserRoleRequest request, CancellationToken ct);

    /// <summary>Owner sets a new password directly for another user - see ResetPasswordRequest.</summary>
    Task<ServiceError> ResetPasswordAsync(int id, ResetPasswordRequest request, CancellationToken ct);
}
