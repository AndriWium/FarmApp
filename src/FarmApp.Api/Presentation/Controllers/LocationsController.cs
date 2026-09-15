using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Locations;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LocationsController(ILocationService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> GetById(int id, CancellationToken ct)
    {
        var location = await service.GetByIdAsync(id, ct);
        return location is null ? NotFound() : location;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<LocationDto>> Create(
        [FromBody] CreateLocationRequest request, IValidator<CreateLocationRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "location");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.LocationId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateLocationRequest request, IValidator<UpdateLocationRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "location");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var error = await service.DeactivateAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "location");
    }
}
