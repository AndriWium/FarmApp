using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.StockTakes;

public interface IStockTakeService
{
    Task<List<StockTakeDto>> GetAllAsync(CancellationToken ct);
    Task<StockTakeDto?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>Creates the StockTake header and one StockTakeLine per given batch id, with
    /// SystemQty snapshotted immediately from the movement ledger. CountedQty/Variance are left
    /// null/zero until RecordCountsAsync is called.</summary>
    Task<ServiceResult<StockTakeDto>> StartStockTakeAsync(StartStockTakeRequest request, CancellationToken ct);

    /// <summary>Records physical counts against a started stock take's lines: sets CountedQty,
    /// computes Variance = CountedQty - SystemQty server-side, and - for every line whose
    /// Variance is non-zero - writes one signed Adjustment movement against that line's batch
    /// (positive if counted more, negative if counted less). One SaveChangesAsync for the whole
    /// batch of line updates + movements.</summary>
    Task<ServiceResult<StockTakeDto>> RecordCountsAsync(int stockTakeId, RecordCountsRequest request, CancellationToken ct);
}
