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

    /// <summary>Records a single signed Adjustment movement against one already-known batch - no
    /// FIFO allocation, unlike RecordAsync(Adjustment, ...): the caller (stock-take reconciliation)
    /// already knows exactly which batch to adjust, so there's no product/grade to allocate
    /// across. Positive increases the batch's on-hand (counted more than the system thought),
    /// negative decreases it (counted less) - both directions, unlike the downward-only
    /// RecordAsync(Adjustment, ...) path the six depletion endpoints still use. Deliberately does
    /// NOT call SaveChangesAsync: the caller controls the transaction boundary (a stock take
    /// commits every line's CountedQty/Variance update and every resulting movement in one
    /// atomic save).</summary>
    Task<StockMovementDto> RecordBatchAdjustmentAsync(int stockBatchId, decimal signedQty, string? reason, int? locationId, CancellationToken ct);

    /// <summary>FIFO-depletes qtyBaseUnits (already converted from pack units if the sale line
    /// sold a pack - the caller's job, not this method's) for one Sale line, writing one SaleOut
    /// movement per batch touched (RefTable "Sale"/RefId saleId, for traceability). Returns each
    /// touched batch's id/qty/UnitCost so the caller (Sales.SaleService) can compute that line's
    /// weighted-average CostAtSale. Deliberately does NOT call SaveChangesAsync - same contract
    /// as RecordBatchAdjustmentAsync: SaleService controls the transaction boundary for the whole
    /// sale (header + every line's movements + every payment, one atomic commit).</summary>
    Task<ServiceResult<SaleDepletionResult>> RecordSaleDepletionAsync(
        int productId, int? gradeId, decimal qtyBaseUnits, int saleId, int? locationId, CancellationToken ct);

    /// <summary>The inverse of RecordSaleDepletionAsync, for Sales.SaleService.RefundSaleAsync:
    /// finds every SaleOut movement this sale originally created (by RefTable "Sale"/RefId
    /// saleId) and writes one offsetting Adjustment movement per batch, crediting back the exact
    /// quantity that was taken - never re-running FIFO allocation, since which batches were
    /// touched is already a matter of historical fact, not a new allocation decision. Reuses the
    /// existing Adjustment movement type (same as StockTake's variance corrections) rather than a
    /// new enum value - see DECISIONS.md. Deliberately does NOT call SaveChangesAsync - same
    /// contract as RecordSaleDepletionAsync: the caller (SaleService.RefundSaleAsync) controls the
    /// transaction boundary for the whole refund (movements + Sale.Status, one atomic
    /// commit).</summary>
    Task<List<StockMovementDto>> ReverseSaleDepletionAsync(int saleId, string? reason, CancellationToken ct);
}
