using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Prices;

public interface IPriceService
{
    Task<ServiceResult<PriceDto>> SetPriceAsync(SetPriceRequest request, CancellationToken ct);
    Task<PriceDto?> GetCurrentAsync(int priceListId, int productId, int? gradeId, int? packSizeId, CancellationToken ct);
    Task<List<PriceDto>> GetHistoryAsync(int priceListId, int productId, int? gradeId, int? packSizeId, CancellationToken ct);
}
