using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.TillSessions;

public interface ITillSessionService
{
    Task<TillSessionDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<TillSessionDto>> GetAllAsync(int? locationId, bool openOnly, CancellationToken ct);

    /// <summary>Opens a new TillSession for locationId, rejecting if one is already open
    /// (ClosedAt == null) for that location - one open session per location at a time (task
    /// brief). openedByUserId is the authenticated caller's AppUserId, resolved by the controller
    /// from the JWT claims - never trust a client-supplied value for who opened a till.</summary>
    Task<ServiceResult<TillSessionDto>> OpenAsync(int locationId, int openedByUserId, CancellationToken ct);

    /// <summary>Day close (doc 01 Module 4 / doc 02): rejects if the session doesn't exist or is
    /// already closed. SystemCardTotal is computed server-side as SUM(SalePayment.Amount) across
    /// every Complete Sale in this TillSession whose payment Method is Card (Refunded sales
    /// excluded - ISalePaymentRepository.GetCardTotalForTillSessionAsync); Difference is
    /// SystemCardTotal - cardMachineBatchTotal (positive = over, negative = short).
    /// cardMachineBatchTotal/differenceNote are the only client-supplied values.</summary>
    Task<ServiceResult<TillSessionDto>> CloseAsync(
        int tillSessionId, decimal cardMachineBatchTotal, string? differenceNote, CancellationToken ct);
}
