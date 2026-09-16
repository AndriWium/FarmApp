using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.StockTakes;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/stock-takes")]
public class StockTakesController(IStockTakeService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<StockTakeDto>>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StockTakeDto>> GetById(int id, CancellationToken ct)
    {
        var stockTake = await service.GetByIdAsync(id, ct);
        return stockTake is null ? NotFound() : stockTake;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<StockTakeDto>> Start(
        [FromBody] StartStockTakeRequest request, IValidator<StartStockTakeRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.StartStockTakeAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "location or stock batch");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.StockTakeId }, result.Value);
    }

    [HttpPost("{id:int}/counts")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<StockTakeDto>> RecordCounts(
        int id, [FromBody] RecordCountsRequest request, IValidator<RecordCountsRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.RecordCountsAsync(id, request, ct);
        if (result.Error != ServiceError.None)
            return result.Detail is not null
                ? ErrorResult(result.Error, "stock take line", result.Detail)
                : ErrorResult(result.Error, "stock take line");

        return result.Value!;
    }
}
