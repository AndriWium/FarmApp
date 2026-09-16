using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.PriceLists;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

// Explicit kebab-case route, matching the fix already applied to ExpenseCategoriesController/
// ActivityTypesController (Phase 5b-1/5b-2): the [controller] token substitutes the PascalCase
// class-name prefix ("PriceLists") verbatim, which never matches a kebab-case request path
// ("price-lists") no matter how case-insensitive ASP.NET Core's routing is - the mismatch is the
// literal hyphen, not casing. Found live while wiring up this entity's Angular screen (every GET
// and POST 404'd until this was added); fixed minimally here rather than worked around client-side.
[ApiController]
[Route("api/v1/price-lists")]
public class PriceListsController(IPriceListService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PriceListDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PriceListDto>> GetById(int id, CancellationToken ct)
    {
        var priceList = await service.GetByIdAsync(id, ct);
        return priceList is null ? NotFound() : priceList;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<PriceListDto>> Create(
        [FromBody] CreatePriceListRequest request, IValidator<CreatePriceListRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "price list");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.PriceListId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdatePriceListRequest request, IValidator<UpdatePriceListRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "price list");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var error = await service.DeactivateAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "price list");
    }
}
