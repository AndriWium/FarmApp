using System.Security.Claims;
using FarmApp.Api.Application.AccountingPeriods;
using FarmApp.Api.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Month-end close/reopen (doc 10 §1, Phase 4c). Every action requires
/// CanManageMasterData (Owner) - the task brief only explicitly named this policy for Reopen
/// ("Owner role only"), but closing a month (and even just viewing its checklist) is the same
/// class of financial-control action, not an operational day-to-day read - doc 13 defines no more
/// specific policy for month-end close, and the task brief says not to invent a new one for a
/// single action (see DECISIONS.md for the judgment call to extend this to all three).</summary>
[ApiController]
[Route("api/v1/accounting-periods")]
[Authorize(Policy = "CanManageMasterData")]
public class AccountingPeriodsController(IAccountingPeriodCloseService service) : ApiControllerBase
{
    [HttpGet("{year:int}/{month:int}/checklist")]
    public async Task<ActionResult<CloseChecklistDto>> GetChecklist(int year, int month, CancellationToken ct)
        => Ok(await service.GetCloseChecklistAsync(year, month, ct));

    /// <summary>ClosedBy is always resolved from the caller's own JWT claims, never from the
    /// request body - matches TillSession.OpenedBy's precedent (a client can't close a period as
    /// somebody else).</summary>
    [HttpPost("{year:int}/{month:int}/close")]
    public async Task<ActionResult<CloseMonthResultDto>> Close(int year, int month, CancellationToken ct)
    {
        var userName = User.FindFirstValue(ClaimTypes.Name);
        if (userName is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Missing or invalid user identity.");

        var result = await service.CloseMonthAsync(year, month, userName, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "accounting period", result.Detail ?? "");

        return result.Value!;
    }

    [HttpPost("{year:int}/{month:int}/reopen")]
    public async Task<ActionResult<AccountingPeriodDto>> Reopen(
        int year, int month, [FromBody] ReopenMonthRequest request, IValidator<ReopenMonthRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return ValidationProblem(validation);

        var result = await service.ReopenMonthAsync(year, month, request.Reason, ct);
        if (result.Error != ServiceError.None) return ErrorResult(result.Error, "accounting period");

        return result.Value!;
    }
}
