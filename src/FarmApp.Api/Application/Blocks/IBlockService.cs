using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Blocks;

public interface IBlockService
{
    Task<List<BlockDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<BlockDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<BlockDto>> CreateAsync(CreateBlockRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateBlockRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
