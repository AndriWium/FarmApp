namespace FarmApp.Api.Application.Users;

/// <summary>Never includes PasswordHash - a user list is personnel information (gated behind
/// CanManageUsers), not a place to leak even a hashed credential over the wire.</summary>
public record UserDto(int AppUserId, string UserName, string Role, bool IsActive);

public record CreateUserRequest(string UserName, string Password, string Role);

public record SetUserActiveRequest(bool IsActive);

public record UpdateUserRoleRequest(string Role);

/// <summary>Owner sets a new password directly for another user - there's no email
/// infrastructure in this app, so this is the pragmatic in-person equivalent of "forgot
/// password" for a small farm team. Deliberately carries no OldPassword - see DECISIONS.md.</summary>
public record ResetPasswordRequest(string NewPassword);
