using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.ExpenseCategories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/expense-categories")]
public class ExpenseCategoriesController(IExpenseCategoryService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExpenseCategoryDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseCategoryDto>> GetById(int id, CancellationToken ct)
    {
        var category = await service.GetByIdAsync(id, ct);
        return category is null ? NotFound() : category;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<ExpenseCategoryDto>> Create(
        [FromBody] CreateExpenseCategoryRequest request, IValidator<CreateExpenseCategoryRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "expense category");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ExpenseCategoryId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateExpenseCategoryRequest request, IValidator<UpdateExpenseCategoryRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "expense category");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var error = await service.DeactivateAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "expense category");
    }
}
