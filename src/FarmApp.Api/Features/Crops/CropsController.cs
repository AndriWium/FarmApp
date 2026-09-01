using FluentValidation;
using FarmApp.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Features.Crops;

[ApiController]
[Route("api/v1/[controller]")]
public class CropsController(ICropService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CropDto>>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

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
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return ValidationProblem(result);

        var dto = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.CropId }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] CreateCropRequest request, IValidator<CreateCropRequest> validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return ValidationProblem(result);

        return await service.UpdateAsync(id, request, ct) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
