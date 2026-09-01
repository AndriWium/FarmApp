using FluentValidation;
using FarmApp.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Features.Blocks;

[ApiController]
[Route("api/v1/[controller]")]
public class BlocksController(IBlockService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BlockDto>>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

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
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return ValidationProblem(result);

        var dto = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.BlockId }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] CreateBlockRequest request, IValidator<CreateBlockRequest> validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return ValidationProblem(result);

        return await service.UpdateAsync(id, request, ct) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
