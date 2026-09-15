using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.ActivityTypes;

public interface IActivityTypeService
{
    Task<List<ActivityTypeDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<ActivityTypeDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<ActivityTypeDto>> CreateAsync(CreateActivityTypeRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateActivityTypeRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
