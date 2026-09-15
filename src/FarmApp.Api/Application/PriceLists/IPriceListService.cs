using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.PriceLists;

public interface IPriceListService
{
    Task<List<PriceListDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<PriceListDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<PriceListDto>> CreateAsync(CreatePriceListRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdatePriceListRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
