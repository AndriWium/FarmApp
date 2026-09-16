using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.SeasonCosting;
using FarmApp.Api.Application.Seasons;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Same auth treatment as PlantingsController - day-to-day farm capture, no
/// CanManageMasterData gate (Phase 3a task brief; see DECISIONS.md). The cost-preview/close
/// endpoints (Phase 4a) stay ungated too - closing a season is a day-to-day farming-cycle event
/// (end of picking), not master-data administration, matching the rest of this controller.</summary>
[ApiController]
[Route("api/v1/seasons")]
public class SeasonsController(ISeasonService service, ISeasonCostingService costingService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SeasonDto>>> GetAll([FromQuery] int? plantingId, CancellationToken ct)
        => await service.GetAllAsync(plantingId, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SeasonDto>> GetById(int id, CancellationToken ct)
    {
        var season = await service.GetByIdAsync(id, ct);
        return season is null ? NotFound() : season;
    }

    [HttpPost]
    public async Task<ActionResult<SeasonDto>> Create(
        [FromBody] CreateSeasonRequest request, IValidator<CreateSeasonRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "planting");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.SeasonId }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateSeasonRequest request, IValidator<UpdateSeasonRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        // "season", not "planting" - unlike Create (whose only failure is the referenced
        // Planting FK), Update can now also fail with SeasonAlreadyClosed, which is unambiguously
        // about the season itself (Phase 4a).
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "season");
    }

    /// <summary>Read-only, callable any number of times while the season is open - writes
    /// nothing (doc 09). Lets a caller see the actual CostPerKg/TrueUpAmount before deciding to
    /// call POST .../close.</summary>
    [HttpGet("{id:int}/cost-preview")]
    public async Task<ActionResult<SeasonCostPreviewDto>> GetCostPreview(int id, CancellationToken ct)
    {
        var result = await costingService.PreviewCloseAsync(id, ct);
        return result.Error != ServiceError.None ? ErrorResult(result.Error, "season") : result.Value!;
    }

    /// <summary>The user-confirmed posting step (doc 09: "show the number, owner clicks
    /// approve") - persists the SeasonCostSummary true-up and closes the season. Returns the same
    /// summary it persists, so the response itself is the confirmation record regardless of
    /// whether the caller actually looked at GetCostPreview first.</summary>
    [HttpPost("{id:int}/close")]
    public async Task<ActionResult<SeasonCostSummaryDto>> Close(int id, CancellationToken ct)
    {
        var result = await costingService.ConfirmCloseAsync(id, ct);
        return result.Error != ServiceError.None ? ErrorResult(result.Error, "season") : result.Value!;
    }
}
