using FarmApp.Api.Application.Blocks;
using FarmApp.Api.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BlocksController(IBlockService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BlockDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BlockDto>> GetById(int id, CancellationToken ct)
    {
        var block = await service.GetByIdAsync(id, ct);
        return block is null ? NotFound() : block;
    }

    [HttpPost]
    public async Task<ActionResult<BlockDto>> Create(
        [FromBody] CreateBlockRequest request, IValidator<CreateBlockRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "block");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.BlockId }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateBlockRequest request, IValidator<UpdateBlockRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "block");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var error = await service.DeactivateAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "block");
    }
}
