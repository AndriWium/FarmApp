using FarmApp.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Features.Crops;

[ApiController]
[Route("api/v1/[controller]")]
public class CropsController(ICropService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CropDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CropDto>> GetById(int id, CancellationToken ct)
    {
        var crop = await service.GetByIdAsync(id, ct);
        return crop is null ? NotFound() : crop;
    }

    [HttpPost]
    public async Task<ActionResult<CropDto>> Create(
        [FromBody] CreateCropRequest request, IValidator<CreateCropRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "crop");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.CropId }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateCropRequest request, IValidator<UpdateCropRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "crop");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var error = await service.DeactivateAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "crop");
    }
}
