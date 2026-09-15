using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Prices;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

// Prices are never updated or deleted, only superseded - so this controller deliberately has
// no PUT/DELETE, only POST (SetPrice, an upsert-with-history) and two read endpoints.
[ApiController]
[Route("api/v1/prices")]
public class PricesController(IPriceService service) : ApiControllerBase
{
    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<PriceDto>> SetPrice(
        [FromBody] SetPriceRequest request, IValidator<SetPriceRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.SetPriceAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "price list, product, grade or pack size");

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("current")]
    public async Task<ActionResult<PriceDto>> GetCurrent(
        [FromQuery] int priceListId, [FromQuery] int productId,
        [FromQuery] int? gradeId, [FromQuery] int? packSizeId, CancellationToken ct)
    {
        var price = await service.GetCurrentAsync(priceListId, productId, gradeId, packSizeId, ct);
        return price is null ? NotFound() : price;
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<PriceDto>>> GetHistory(
        [FromQuery] int priceListId, [FromQuery] int productId,
        [FromQuery] int? gradeId, [FromQuery] int? packSizeId, CancellationToken ct)
        => await service.GetHistoryAsync(priceListId, productId, gradeId, packSizeId, ct);
}
