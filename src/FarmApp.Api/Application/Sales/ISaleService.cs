using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Sales;

public interface ISaleService
{
    Task<SaleDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<SaleDto>> GetAllAsync(int? tillSessionId, CancellationToken ct);

    /// <summary>The full checkout flow (task brief): idempotency check on ClientGuid first (a
    /// replay returns the existing sale untouched, never reprocesses - CreateSaleResult.WasReplay
    /// tells the controller to answer 200 instead of 201, per doc 08); validates the till session
    /// is open, the customer (if any) exists, every line's product/grade/pack-size exists, that
    /// an Account payment always carries a CustomerId, and that payments exactly cover the
    /// computed total; then, inside one real EF Core transaction, FIFO-depletes each line's
    /// base-unit quantity, snapshots each line's weighted-average CostAtSale, and writes the
    /// Sale + SaleLines + SalePayments together. Any failure from this point (most notably
    /// insufficient stock) rolls the whole transaction back - nothing about the sale survives
    /// partially.</summary>
    Task<ServiceResult<CreateSaleResult>> CreateSaleAsync(CreateSaleRequest request, CancellationToken ct);

    /// <summary>Reverses a Complete sale (task brief): rejects if the sale doesn't exist or is
    /// already Refunded. Inside one real EF Core transaction (same pattern as CreateSaleAsync),
    /// credits back the exact StockBatch quantities the sale's checkout originally depleted (via
    /// IStockMovementService.ReverseSaleDepletionAsync - no FIFO re-allocation, since exactly
    /// which batches were touched is already known) and flips Sale.Status to Refunded. Does NOT
    /// create, delete, or modify any SalePayment row - the original payment stays as historical
    /// fact of what was charged; a Card refund's actual money movement happens on the physical
    /// card machine, outside this system. Day-close and customer-balance calculations already
    /// exclude Refunded sales (Sale.Status == Complete filters), which is what makes this correct
    /// without reversing payment rows.</summary>
    Task<ServiceResult<SaleDto>> RefundSaleAsync(int saleId, string? reason, CancellationToken ct);
}
