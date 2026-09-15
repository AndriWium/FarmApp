using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.StockBatches;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StockBatchesController(IStockBatchService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<StockBatchDto>>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StockBatchDto>> GetById(int id, CancellationToken ct)
    {
        var batch = await service.GetByIdAsync(id, ct);
        return batch is null ? NotFound() : batch;
    }

    [HttpGet("{id:int}/on-hand")]
    public async Task<ActionResult<decimal>> GetOnHand(int id, CancellationToken ct)
    {
        var onHand = await service.GetOnHandAsync(id, ct);
        return onHand is null ? NotFound() : onHand.Value;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<StockBatchDto>> Create(
        [FromBody] CreateStockBatchRequest request, IValidator<CreateStockBatchRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "product or grade");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.StockBatchId }, result.Value);
    }
}
