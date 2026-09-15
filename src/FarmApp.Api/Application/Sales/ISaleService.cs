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
}
