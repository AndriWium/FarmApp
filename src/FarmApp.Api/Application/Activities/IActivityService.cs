using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Activities;

public interface IActivityService
{
    Task<List<ActivityDto>> GetAllAsync(int? seasonId, CancellationToken ct);
    Task<ActivityDto?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>Validates Season/ActivityType and every input line's InputItemId first (nothing
    /// is written if any reference is bad), then - inside one real EF Core transaction - inserts
    /// the Activity header and, per input line, checks on-hand, snapshots the weighted-average
    /// cost, writes the ActivityInput row, and depletes a Consumption InputStockMovement. Any
    /// line that would deplete more than what's on hand rejects the whole activity
    /// (ServiceError.InsufficientStock) with nothing partially written (task brief).</summary>
    Task<ServiceResult<ActivityDto>> CreateActivityAsync(CreateActivityRequest request, CancellationToken ct);
}
