using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Locations;

public interface ILocationService
{
    Task<List<LocationDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<LocationDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<LocationDto>> CreateAsync(CreateLocationRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateLocationRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
