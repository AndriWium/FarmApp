using FarmApp.Api.Application.ActivityTypes;
using FarmApp.Api.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>ActivityType is master data (like Grade/Block/Crop) - gated behind
/// CanManageMasterData, unlike Planting/Season/Activity/RainfallLog which record day-to-day
/// farm events (Phase 3a task brief).</summary>
[ApiController]
[Route("api/v1/activity-types")]
public class ActivityTypesController(IActivityTypeService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ActivityTypeDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ActivityTypeDto>> GetById(int id, CancellationToken ct)
    {
        var activityType = await service.GetByIdAsync(id, ct);
        return activityType is null ? NotFound() : activityType;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<ActivityTypeDto>> Create(
        [FromBody] CreateActivityTypeRequest request, IValidator<CreateActivityTypeRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "activity type");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ActivityTypeId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateActivityTypeRequest request, IValidator<UpdateActivityTypeRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "activity type");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var error = await service.DeactivateAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "activity type");
    }
}
