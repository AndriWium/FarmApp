using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Seasons;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Same auth treatment as PlantingsController - day-to-day farm capture, no
/// CanManageMasterData gate (Phase 3a task brief; see DECISIONS.md).</summary>
[ApiController]
[Route("api/v1/seasons")]
public class SeasonsController(ISeasonService service) : ApiControllerBase
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
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "planting");
    }
}
