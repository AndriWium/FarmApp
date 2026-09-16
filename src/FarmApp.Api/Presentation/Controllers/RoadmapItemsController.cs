using FarmApp.Api.Application.RoadmapItems;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>The "Up & Coming" tab (AI Guide/14-up-and-coming.md) - reads open to any authenticated
/// user (no CanViewReports/CanManageMasterData gate on the GETs: "all roles see it"), writes
/// Owner-only ("Owner-only CRUD screen").</summary>
[ApiController]
[Route("api/v1/[controller]")]
public class RoadmapItemsController(IRoadmapItemService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RoadmapItemDto>>> GetAll(CancellationToken ct)
        => await service.GetAllAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoadmapItemDto>> GetById(int id, CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return item is null ? NotFound() : item;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<ActionResult<RoadmapItemDto>> Create(
        [FromBody] CreateRoadmapItemRequest request, IValidator<CreateRoadmapItemRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.RoadmapItemId }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageMasterData")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateRoadmapItemRequest request, IValidator<UpdateRoadmapItemRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var found = await service.UpdateAsync(id, request, ct);
        return found ? NoContent() : NotFound();
    }
}
