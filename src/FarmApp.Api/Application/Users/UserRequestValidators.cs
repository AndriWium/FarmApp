using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.Users;

/// <summary>The three roles this app recognizes today (AppUser.Role, a plain string - doc 03).
/// Kept here as the single source of truth for validators that need to check against it.</summary>
internal static class AppUserRoles
{
    public static readonly string[] Valid = ["Owner", "Cashier", "Worker"];
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.UserName).RequiredName();   // NotEmpty + MaxLength(50), matches AppUser.UserName's DB column
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => AppUserRoles.Valid.Contains(r))
            .WithMessage("Role must be one of: Owner, Cashier, Worker.");
    }
}

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => AppUserRoles.Valid.Contains(r))
            .WithMessage("Role must be one of: Owner, Cashier, Worker.");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}
