using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Sales;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Checkout is a Cashier action, not Owner-only (task brief) - no
/// [Authorize(Policy = "CanManageMasterData")] here, just the fallback policy (any authenticated
/// user, Program.cs). All the actual business rules (till must be open, payments must cover the
/// total, stock must be available, etc.) live in ISaleService - this controller only translates
/// HTTP in/out (doc 11).</summary>
[ApiController]
[Route("api/v1/sales")]
public class SalesController(ISaleService service) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SaleDto>> Create(
        [FromBody] CreateSaleRequest request, IValidator<CreateSaleRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateSaleAsync(request, ct);
        if (result.Error != ServiceError.None)
        {
            return result.Detail is null
                ? ErrorResult(result.Error, "till session, customer, product, grade, or pack size")
                : ErrorResult(result.Error, "till session, customer, product, grade, or pack size", result.Detail);
        }

        var (sale, wasReplay) = result.Value!;
        // A replayed ClientGuid (doc 08) did nothing new - the existing sale comes back as 200,
        // not 201, so a retrying client can tell "already had this" from "just created this".
        return wasReplay
            ? Ok(sale)
            : CreatedAtAction(nameof(GetById), new { id = sale.SaleId }, sale);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleDto>> GetById(int id, CancellationToken ct)
    {
        var sale = await service.GetByIdAsync(id, ct);
        return sale is null ? NotFound() : sale;
    }

    [HttpGet]
    public async Task<ActionResult<List<SaleDto>>> GetAll([FromQuery] int? tillSessionId, CancellationToken ct)
        => await service.GetAllAsync(tillSessionId, ct);
}
