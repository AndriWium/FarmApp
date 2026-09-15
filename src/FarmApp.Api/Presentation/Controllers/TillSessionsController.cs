using System.Security.Claims;
using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.TillSessions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Opening a till session is a Cashier action, not Owner-only (task brief) - no
/// [Authorize(Policy = "CanManageMasterData")] here, just the fallback policy (any authenticated
/// user) already wired in Program.cs. OpenedBy is always resolved from the caller's own JWT
/// claims, never from the request body - a client can't open a session on someone else's
/// behalf.</summary>
[ApiController]
[Route("api/v1/till-sessions")]
public class TillSessionsController(ITillSessionService service) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TillSessionDto>> Open(
        [FromBody] OpenTillSessionRequest request, IValidator<OpenTillSessionRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Missing or invalid user identity.");

        var result = await service.OpenAsync(request.LocationId, userId, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "location");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.TillSessionId }, result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TillSessionDto>> GetById(int id, CancellationToken ct)
    {
        var session = await service.GetByIdAsync(id, ct);
        return session is null ? NotFound() : session;
    }

    [HttpGet]
    public async Task<ActionResult<List<TillSessionDto>>> GetAll(
        [FromQuery] int? locationId, [FromQuery] bool openOnly, CancellationToken ct)
        => await service.GetAllAsync(locationId, openOnly, ct);

    /// <summary>Day close (doc 01 Module 4 / doc 02): compares the system's own card-sales total
    /// against the card machine's settlement batch total and records the difference. Same
    /// fallback-policy access as Open - closing out a till is a Cashier action too, not
    /// Owner-only.</summary>
    [HttpPost("{id:int}/close")]
    public async Task<ActionResult<TillSessionDto>> Close(
        int id, [FromBody] CloseTillSessionRequest request, IValidator<CloseTillSessionRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.CloseAsync(id, request.CardMachineBatchTotal, request.DifferenceNote, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "till session");

        return result.Value!;
    }
}
