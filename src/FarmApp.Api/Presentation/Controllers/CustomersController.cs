using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Customers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

// This is the first entity in the codebase holding real personal information (a person's
// name and phone number). Writes stay [FromBody] (never [FromQuery]/route) so PII never
// lands in a URL or query string - the established convention, just deliberately preserved
// here rather than adding e.g. a search-by-phone query endpoint.
[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController(ICustomerService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id, CancellationToken ct)
    {
        var customer = await service.GetByIdAsync(id, ct);
        return customer is null ? NotFound() : customer;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<CustomerDto>> Create(
        [FromBody] CreateCustomerRequest request, IValidator<CreateCustomerRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "customer");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.CustomerId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateCustomerRequest request, IValidator<UpdateCustomerRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "customer");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var error = await service.DeactivateAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "customer");
    }
}
