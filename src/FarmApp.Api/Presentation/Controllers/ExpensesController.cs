using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Expenses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Recording a farm expense (fuel, repairs, wages) - deny-by-default fallback policy
/// only, no [Authorize(Policy = "CanManageMasterData")] (Phase 4b task brief): routine day-to-day
/// capture, same shape as Activity/Harvest, not master-data administration. Plain create + read -
/// no update/delete, an expense is a historical fact once recorded (matches CustomerPayment's
/// precedent).</summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ExpensesController(IExpenseService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExpenseDto>>> GetAll(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? categoryId, CancellationToken ct)
        => await service.GetAllAsync(from, to, categoryId, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> GetById(int id, CancellationToken ct)
    {
        var expense = await service.GetByIdAsync(id, ct);
        return expense is null ? NotFound() : expense;
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(
        [FromBody] CreateExpenseRequest request, IValidator<CreateExpenseRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "expense category, supplier, or season");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ExpenseId }, result.Value);
    }
}
