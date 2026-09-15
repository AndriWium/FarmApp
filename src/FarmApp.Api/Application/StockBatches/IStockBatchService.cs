using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.StockBatches;

public interface IStockBatchService
{
    Task<List<StockBatchDto>> GetAllAsync(CancellationToken ct);
    Task<StockBatchDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<decimal?> GetOnHandAsync(int id, CancellationToken ct);
    Task<ServiceResult<StockBatchDto>> CreateAsync(CreateStockBatchRequest request, CancellationToken ct);
}
