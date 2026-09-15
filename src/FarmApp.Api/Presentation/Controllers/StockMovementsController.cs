using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.StockMovements;
using FarmApp.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Records non-header stock movements. Each write endpoint fixes its own
/// StockMovementType and delegates the FIFO batch depletion to IStockMovementService — one
/// action per movement type (doc 11: "your call, just keep it discoverable and consistent"),
/// since the request shape is identical across all six but the type is not something a caller
/// should be free to choose (that would let a client mislabel a wastage as a donation).
/// CanManageMasterData on writes for now — a more precise CanManageStock policy would be better
/// long-term (see DECISIONS.md); the on-hand read follows the rest of the app's GET convention
/// (any authenticated user, no attribute needed — see the fallback policy in Program.cs).</summary>
[ApiController]
[Route("api/v1/stock-movements")]
public class StockMovementsController(IStockMovementService service) : ApiControllerBase
{
    [HttpPost("wastage")]
    [Authorize(Policy = "CanManageMasterData")]
    public Task<ActionResult<List<StockMovementDto>>> Wastage(
        [FromBody] RecordStockMovementRequest request, IValidator<RecordStockMovementRequest> validator, CancellationToken ct)
        => Record(StockMovementType.Wastage, request, validator, ct);

    [HttpPost("own-use")]
    [Authorize(Policy = "CanManageMasterData")]
    public Task<ActionResult<List<StockMovementDto>>> OwnUse(
        [FromBody] RecordStockMovementRequest request, IValidator<RecordStockMovementRequest> validator, CancellationToken ct)
        => Record(StockMovementType.OwnUse, request, validator, ct);

    [HttpPost("sample")]
    [Authorize(Policy = "CanManageMasterData")]
    public Task<ActionResult<List<StockMovementDto>>> Sample(
        [FromBody] RecordStockMovementRequest request, IValidator<RecordStockMovementRequest> validator, CancellationToken ct)
        => Record(StockMovementType.Sample, request, validator, ct);

    [HttpPost("donation")]
    [Authorize(Policy = "CanManageMasterData")]
    public Task<ActionResult<List<StockMovementDto>>> Donation(
        [FromBody] RecordStockMovementRequest request, IValidator<RecordStockMovementRequest> validator, CancellationToken ct)
        => Record(StockMovementType.Donation, request, validator, ct);

    [HttpPost("adjustment")]
    [Authorize(Policy = "CanManageMasterData")]
    public Task<ActionResult<List<StockMovementDto>>> Adjustment(
        [FromBody] RecordStockMovementRequest request, IValidator<RecordStockMovementRequest> validator, CancellationToken ct)
        => Record(StockMovementType.Adjustment, request, validator, ct);

    [HttpPost("repack")]
    [Authorize(Policy = "CanManageMasterData")]
    public Task<ActionResult<List<StockMovementDto>>> Repack(
        [FromBody] RecordStockMovementRequest request, IValidator<RecordStockMovementRequest> validator, CancellationToken ct)
        => Record(StockMovementType.Repack, request, validator, ct);

    [HttpPost("transfer")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<List<StockMovementDto>>> Transfer(
        [FromBody] TransferStockRequest request, IValidator<TransferStockRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.TransferAsync(request, ct);
        if (result.Error != ServiceError.None)
            return result.Detail is null ? ErrorResult(result.Error, "stock") : ErrorResult(result.Error, "stock", result.Detail);

        return result.Value!;
    }

    [HttpGet("on-hand")]
    public async Task<ActionResult<List<StockOnHandSummaryDto>>> OnHand(CancellationToken ct)
        => await service.GetOnHandSummaryAsync(ct);

    private async Task<ActionResult<List<StockMovementDto>>> Record(
        StockMovementType type, RecordStockMovementRequest request, IValidator<RecordStockMovementRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.RecordAsync(type, request, ct);
        if (result.Error != ServiceError.None)
            return result.Detail is null ? ErrorResult(result.Error, "stock") : ErrorResult(result.Error, "stock", result.Detail);

        return result.Value!;
    }
}
