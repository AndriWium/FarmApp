using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Products;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController(IProductService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
        => await service.GetAllAsync(includeInactive, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
    {
        var product = await service.GetByIdAsync(id, ct);
        return product is null ? NotFound() : product;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request, IValidator<CreateProductRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "product");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ProductId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateProductRequest request, IValidator<UpdateProductRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "product");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var error = await service.DeactivateAsync(id, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "product");
    }
}
