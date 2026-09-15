using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.PackSizes;

public interface IPackSizeService
{
    Task<List<PackSizeDto>> GetAllAsync(CancellationToken ct);
    Task<PackSizeDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<PackSizeDto>> CreateAsync(CreatePackSizeRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdatePackSizeRequest request, CancellationToken ct);
    Task<ServiceError> DeleteAsync(int id, CancellationToken ct);
}
