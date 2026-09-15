using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.InputPurchases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Recording an input purchase (fertiliser/seed/chemicals/packaging) is stock/master-
/// data administration, same footing as ProducePurchase - gated behind CanManageMasterData,
/// matching that controller's precedent (see DECISIONS.md; distinct from Planting/Season/
/// Activity/RainfallLog, which the Phase 3a task brief deliberately leaves ungated).</summary>
[ApiController]
[Route("api/v1/input-purchases")]
public class InputPurchasesController(IInputPurchaseService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<InputPurchaseDto>>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InputPurchaseDto>> GetById(int id, CancellationToken ct)
    {
        var purchase = await service.GetByIdAsync(id, ct);
        return purchase is null ? NotFound() : purchase;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<InputPurchaseDto>> Create(
        [FromBody] CreateInputPurchaseRequest request, IValidator<CreateInputPurchaseRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreatePurchaseAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "supplier or input item");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.InputPurchaseId }, result.Value);
    }
}
