using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.RainfallLogs;

public interface IRainfallLogService
{
    Task<List<RainfallLogDto>> GetAllAsync(DateOnly? from, DateOnly? to, CancellationToken ct);
    Task<RainfallLogDto?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>Rejects a duplicate Date (ServiceError.DuplicateName - one reading per day, task brief).</summary>
    Task<ServiceResult<RainfallLogDto>> CreateAsync(CreateRainfallLogRequest request, CancellationToken ct);

    /// <summary>Corrects a mis-entered Mm/Notes value - Date itself is not editable (it's the
    /// natural key the uniqueness rule is built around; correcting the date is really "delete and
    /// re-add", not supported this phase - task brief's "your call").</summary>
    Task<ServiceError> UpdateAsync(int id, UpdateRainfallLogRequest request, CancellationToken ct);
}
