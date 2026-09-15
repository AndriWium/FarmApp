using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Harvests;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Same auth treatment as ActivitiesController/PlantingsController/SeasonsController -
/// day-to-day farm capture, no CanManageMasterData gate (Phase 3b task brief; see DECISIONS.md).
/// Withholding-period enforcement is a business rule inside IHarvestService, not an auth
/// concern - any authenticated user can record a harvest, but a locked block still requires an
/// override reason regardless of who they are.</summary>
[ApiController]
[Route("api/v1/harvests")]
public class HarvestsController(IHarvestService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<HarvestDto>>> GetAll([FromQuery] int? seasonId, CancellationToken ct)
        => await service.GetAllAsync(seasonId, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HarvestDto>> GetById(int id, CancellationToken ct)
    {
        var harvest = await service.GetByIdAsync(id, ct);
        return harvest is null ? NotFound() : harvest;
    }

    [HttpPost]
    public async Task<ActionResult<HarvestDto>> Create(
        [FromBody] CreateHarvestRequest request, IValidator<CreateHarvestRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateHarvestAsync(request, ct);
        if (result.Error != ServiceError.None)
        {
            return result.Detail is null
                ? ErrorResult(result.Error, "season, product, or grade")
                : ErrorResult(result.Error, "season, product, or grade", result.Detail);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.HarvestId }, result.Value);
    }
}
