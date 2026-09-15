using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.InputItems;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class InputItemsController(IInputItemService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<InputItemDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InputItemDto>> GetById(int id, CancellationToken ct)
    {
        var inputItem = await service.GetByIdAsync(id, ct);
        return inputItem is null ? NotFound() : inputItem;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<InputItemDto>> Create(
        [FromBody] CreateInputItemRequest request, IValidator<CreateInputItemRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "input item");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.InputItemId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateInputItemRequest request, IValidator<UpdateInputItemRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "input item");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var error = await service.DeactivateAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "input item");
    }

    /// <summary>On-hand = SUM(Qty) over this item's InputStockMovement ledger (doc 02) - never a
    /// stored column. No auth gating beyond the fallback policy: reading stock levels is not a
    /// master-data-write concern (Phase 3a task brief).</summary>
    [HttpGet("{id:int}/on-hand")]
    public async Task<ActionResult<decimal>> GetOnHand(int id, CancellationToken ct)
    {
        var onHand = await service.GetOnHandAsync(id, ct);
        return onHand is null ? NotFound() : onHand.Value;
    }

    /// <summary>Weighted-average cost across every PurchaseIn movement for this item - what
    /// ActivityInput.UnitCost snapshots at the moment an input is consumed (Phase 3a task brief).</summary>
    [HttpGet("{id:int}/weighted-average-cost")]
    public async Task<ActionResult<decimal>> GetWeightedAverageCost(int id, CancellationToken ct)
    {
        var wac = await service.GetWeightedAverageCostAsync(id, ct);
        return wac is null ? NotFound() : wac.Value;
    }
}
