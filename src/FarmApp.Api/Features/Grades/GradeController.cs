using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Features.Grades;

public class GradeController(IGradeRepository repo, IUnitOfWork uow) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GradeDto>>> GetAll(CancellationToken ct)
    {
        var grades = await repo.GetAllAsync(ct);
        return grades.Select(g => new GradeDto(g.GradeId, g.Name)).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GradeDto>> GetById(int id, CancellationToken ct)
    {
        var grade = await repo.GetByIdAsync(id, ct);
        return grade is null ? NotFound() : new GradeDto(grade.GradeId, grade.Name);
    }

    [HttpPost]
    public async Task<ActionResult<GradeDto>> Create(CreateGradeRequest request, IValidator<CreateGradeRequest> validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid)//this can be abstracted
        {
            foreach (var e in result.Errors) ModelState.AddModelError(e.PropertyName, e.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var grade = new Grade { Name = request.Name };
        await repo.AddAsync(grade, ct);
        await uow.SaveChangesAsync(ct);
        return CreatedAtAction( //check this out
            nameof(GetById), 
            new { id = grade.GradeId }, 
            new GradeDto(grade.GradeId, grade.Name)
            );
    }

}
