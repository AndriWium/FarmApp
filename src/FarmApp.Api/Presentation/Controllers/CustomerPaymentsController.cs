using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.CustomerPayments;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Recording a payment received against a customer's account - a Cashier/Owner action
/// (no [Authorize(Policy = "CanManageMasterData")], same fallback-policy access as Sales/
/// TillSessions), not master-data management. Plain create + read (task brief) - no update/
/// delete, payments received are historical fact.</summary>
[ApiController]
[Route("api/v1/customer-payments")]
public class CustomerPaymentsController(ICustomerPaymentService service) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CustomerPaymentDto>> Create(
        [FromBody] CreateCustomerPaymentRequest request, IValidator<CreateCustomerPaymentRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "customer");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.CustomerPaymentId }, result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerPaymentDto>> GetById(int id, CancellationToken ct)
    {
        var payment = await service.GetByIdAsync(id, ct);
        return payment is null ? NotFound() : payment;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerPaymentDto>>> GetByCustomer([FromQuery] int customerId, CancellationToken ct)
        => await service.GetByCustomerIdAsync(customerId, ct);
}
