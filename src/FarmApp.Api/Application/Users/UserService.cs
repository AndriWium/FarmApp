using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.AspNetCore.Identity;

namespace FarmApp.Api.Application.Users;

public class UserService(IAppUserRepository repo, IUnitOfWork uow) : IUserService
{
    public Task<List<UserDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(u => new UserDto(u.AppUserId, u.UserName, u.Role, u.IsActive), includeInactive, ct);

    public Task<UserDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, u => new UserDto(u.AppUserId, u.UserName, u.Role, u.IsActive), ct);

    public async Task<ServiceResult<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByUserNameAsync(request.UserName, excludeId: null, ct))
            return ServiceResult<UserDto>.Fail(ServiceError.DuplicateName);

        var user = new AppUser { UserName = request.UserName, Role = request.Role };

        // Hash exactly like AuthController.Login/the Program.cs Owner seed - PasswordHasher<T>
        // (PBKDF2, per-user salt, versioned format), never a home-rolled scheme (doc 13).
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        await repo.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<UserDto>.Ok(new UserDto(user.AppUserId, user.UserName, user.Role, user.IsActive));
    }

    public async Task<ServiceError> SetActiveAsync(int id, bool isActive, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(id, ct);
        if (user is null) return ServiceError.NotFound;

        user.IsActive = isActive;

        // Deactivating a user must also cut off any refresh token already sitting in a browser -
        // otherwise the account could keep silently minting new access tokens after being
        // switched off (see DECISIONS.md).
        if (!isActive)
            await repo.RevokeRefreshTokensAsync(id, ct);

        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> UpdateRoleAsync(int id, UpdateUserRoleRequest request, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(id, ct);
        if (user is null) return ServiceError.NotFound;

        user.Role = request.Role;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> ResetPasswordAsync(int id, ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(id, ct);
        if (user is null) return ServiceError.NotFound;

        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
