using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.RainfallLogs;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>No auth gating beyond the deny-by-default fallback policy (any authenticated user,
/// Program.cs) - recording a daily rainfall reading is dead-simple day-to-day farm capture, the
/// same footing as Planting/Season/Activity (task brief).</summary>
[ApiController]
[Route("api/v1/rainfall-logs")]
public class RainfallLogsController(IRainfallLogService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RainfallLogDto>>> GetAll(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => await service.GetAllAsync(from, to, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RainfallLogDto>> GetById(int id, CancellationToken ct)
    {
        var rainfallLog = await service.GetByIdAsync(id, ct);
        return rainfallLog is null ? NotFound() : rainfallLog;
    }

    [HttpPost]
    public async Task<ActionResult<RainfallLogDto>> Create(
        [FromBody] CreateRainfallLogRequest request, IValidator<CreateRainfallLogRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CreateAsync(request, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "rainfall log");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.RainfallLogId }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateRainfallLogRequest request, IValidator<UpdateRainfallLogRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var error = await service.UpdateAsync(id, request, ct);
        return error == ServiceError.None ? NoContent() : ErrorResult(error, "rainfall log");
    }
}
