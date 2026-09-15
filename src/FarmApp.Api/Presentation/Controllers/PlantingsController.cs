using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Plantings;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Recording a planting is day-to-day farm capture, not master-data administration -
/// deliberately no [Authorize(Policy = "CanManageMasterData")] here, just the fallback policy
/// (any authenticated user, Program.cs), same treatment as TillSession/Sale (Phase 3a task
/// brief; see DECISIONS.md).</summary>
[ApiController]
[Route("api/v1/plantings")]
public class PlantingsController(IPlantingService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PlantingDto>>> GetAll([FromQuery] int? blockId, CancellationToken ct)
        => await service.GetAllAsync(blockId, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlantingDto>> GetById(int id, CancellationToken ct)
    {
        var planting = await service.GetByIdAsync(id, ct);
        return planting is null ? NotFound() : planting;
    }

    [HttpPost]
    public async Task<ActionResult<PlantingDto>> Create(
        [FromBody] CreatePlantingRequest request, IValidator<CreatePlantingRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "block or cultivar");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.PlantingId }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdatePlantingRequest request, IValidator<UpdatePlantingRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "block or cultivar");
    }
}
