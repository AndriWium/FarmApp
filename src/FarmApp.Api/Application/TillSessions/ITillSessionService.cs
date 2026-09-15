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
}
