using FarmApp.Api.Application.Activities;
using FarmApp.Api.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Same auth treatment as PlantingsController/SeasonsController - day-to-day farm
/// capture, no CanManageMasterData gate (Phase 3a task brief; see DECISIONS.md). A Worker role
/// should plausibly be able to log a spray/irrigation/harvest-prep activity, not just an Owner.</summary>
[ApiController]
[Route("api/v1/activities")]
public class ActivitiesController(IActivityService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ActivityDto>>> GetAll([FromQuery] int? seasonId, CancellationToken ct)
        => await service.GetAllAsync(seasonId, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ActivityDto>> GetById(int id, CancellationToken ct)
    {
        var activity = await service.GetByIdAsync(id, ct);
        return activity is null ? NotFound() : activity;
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(
        [FromBody] CreateActivityRequest request, IValidator<CreateActivityRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateActivityAsync(request, ct);
        if (result.Error != ServiceError.None)
        {
            return result.Detail is null
                ? ErrorResult(result.Error, "season, activity type, or input item")
                : ErrorResult(result.Error, "season, activity type, or input item", result.Detail);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ActivityId }, result.Value);
    }
}
