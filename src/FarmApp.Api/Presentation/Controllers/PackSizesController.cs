using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.PackSizes;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PackSizesController(IPackSizeService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PackSizeDto>>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PackSizeDto>> GetById(int id, CancellationToken ct)
    {
        var packSize = await service.GetByIdAsync(id, ct);
        return packSize is null ? NotFound() : packSize;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<PackSizeDto>> Create(
        [FromBody] CreatePackSizeRequest request, IValidator<CreatePackSizeRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "pack size");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.PackSizeId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdatePackSizeRequest request, IValidator<UpdatePackSizeRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "pack size");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var error = await service.DeleteAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "pack size");
    }
}
