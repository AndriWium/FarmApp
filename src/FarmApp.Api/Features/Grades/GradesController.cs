using FluentValidation;
using FarmApp.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Features.Grades;

[ApiController]
[Route("api/v1/[controller]")]
public class GradesController(IGradeService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GradeDto>>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GradeDto>> GetById(int id, CancellationToken ct)
    {
        var grade = await service.GetByIdAsync(id, ct);
        return grade is null ? NotFound() : grade;
    }

    [HttpPost]
    public async Task<ActionResult<GradeDto>> Create(
        [FromBody] CreateGradeRequest request, IValidator<CreateGradeRequest> validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return ValidationProblem(result);

        var dto = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.GradeId }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] CreateGradeRequest request, IValidator<CreateGradeRequest> validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return ValidationProblem(result);

        return await service.UpdateAsync(id, request, ct) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
