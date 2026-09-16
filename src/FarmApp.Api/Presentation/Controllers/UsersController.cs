using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Users;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>User management - creating accounts, deactivating/reactivating, changing role, and
/// resetting a password. Gated Owner-only at the controller level for both reads and writes:
/// listing usernames/roles is personnel information, same "gate the whole controller" precedent
/// AccountingPeriodsController/ReportsController already established (see DECISIONS.md).</summary>
[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = "CanManageUsers")]
public class UsersController(IUserService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken ct)
    {
        var user = await service.GetByIdAsync(id, ct);
        return user is null ? NotFound() : user;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        [FromBody] CreateUserRequest request, IValidator<CreateUserRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "user");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.AppUserId }, result.Value);
    }

    [HttpPut("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetUserActiveRequest request, CancellationToken ct)
    {
        var error = await service.SetActiveAsync(id, request.IsActive, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "user");
    }

    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> UpdateRole(
        int id, [FromBody] UpdateUserRoleRequest request, IValidator<UpdateUserRoleRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateRoleAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "user");
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        int id, [FromBody] ResetPasswordRequest request, IValidator<ResetPasswordRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.ResetPasswordAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "user");
    }
}
