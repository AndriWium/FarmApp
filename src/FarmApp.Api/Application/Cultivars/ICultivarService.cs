using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Cultivars;

public interface ICultivarService
{
    Task<List<CultivarDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<CultivarDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<CultivarDto>> CreateAsync(CreateCultivarRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateCultivarRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
