using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.ProducePurchases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/produce-purchases")]
public class ProducePurchasesController(IProducePurchaseService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProducePurchaseDto>>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProducePurchaseDto>> GetById(int id, CancellationToken ct)
    {
        var purchase = await service.GetByIdAsync(id, ct);
        return purchase is null ? NotFound() : purchase;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<ProducePurchaseDto>> Create(
        [FromBody] CreatePurchaseRequest request, IValidator<CreatePurchaseRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreatePurchaseAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "supplier, product, or grade");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ProducePurchaseId }, result.Value);
    }
}
