using FarmApp.Api.Application.Common;
using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.StockMovements;

public interface IStockMovementService
{
    /// <summary>Records a FIFO-depleted, single-location movement. type must be one of Wastage,
    /// OwnUse, Sample, Donation, Adjustment, Repack — the types this use case covers.</summary>
    Task<ServiceResult<List<StockMovementDto>>> RecordAsync(StockMovementType type, RecordStockMovementRequest request, CancellationToken ct);

    Task<ServiceResult<List<StockMovementDto>>> TransferAsync(TransferStockRequest request, CancellationToken ct);

    Task<List<StockOnHandSummaryDto>> GetOnHandSummaryAsync(CancellationToken ct);
}
