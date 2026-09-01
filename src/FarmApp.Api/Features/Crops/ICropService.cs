using FarmApp.Api.Shared;

namespace FarmApp.Api.Features.Crops;

public interface ICropService
{
    Task<List<CropDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<CropDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<CropDto>> CreateAsync(CreateCropRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateCropRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
