using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.ProducePurchases;

public interface IProducePurchaseService
{
    Task<List<ProducePurchaseDto>> GetAllAsync(CancellationToken ct);
    Task<ProducePurchaseDto?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>Validates the supplier and every line's product/grade first (nothing is written
    /// if any reference is bad), then inserts the header, inserts each line, and - reusing
    /// IStockBatchService.CreateAsync - creates each line's StockBatch (Source: Purchase,
    /// PurchaseLineId set) + seeding PurchaseIn movement.</summary>
    Task<ServiceResult<ProducePurchaseDto>> CreatePurchaseAsync(CreatePurchaseRequest request, CancellationToken ct);
}
