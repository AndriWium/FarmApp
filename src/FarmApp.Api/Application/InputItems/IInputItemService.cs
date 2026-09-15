using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.InputItems;

public interface IInputItemService
{
    Task<List<InputItemDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<InputItemDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<InputItemDto>> CreateAsync(CreateInputItemRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateInputItemRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
