using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Plantings;

public interface IPlantingService
{
    Task<List<PlantingDto>> GetAllAsync(int? blockId, CancellationToken ct);
    Task<PlantingDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<PlantingDto>> CreateAsync(CreatePlantingRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdatePlantingRequest request, CancellationToken ct);
}
