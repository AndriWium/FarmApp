using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.Sales;

public record CreateSaleLineRequest(
    int ProductId, int? GradeId, int? PackSizeId, decimal Qty, decimal UnitPrice,
    decimal DiscountAmount, string? DiscountReason);

public record CreateSalePaymentRequest(SalePaymentMethod Method, decimal Amount);

/// <summary>ClientGuid is client-generated and unique (doc 08) - the idempotency key that makes
/// a flaky-connection retry safe. CreateSaleAsync checks it before touching anything else.</summary>
public record CreateSaleRequest(
    Guid ClientGuid, int TillSessionId, int? CustomerId, SaleChannel Channel, string? Notes,
    List<CreateSaleLineRequest> Lines, List<CreateSalePaymentRequest> Payments);

public record SaleLineDto(
    int SaleLineId, int ProductId, int? GradeId, int? PackSizeId, decimal Qty, decimal UnitPrice,
    decimal DiscountAmount, string? DiscountReason, decimal CostAtSale);

public record SalePaymentDto(int SalePaymentId, SalePaymentMethod Method, decimal Amount);

public record SaleDto(
    int SaleId, int TillSessionId, int? CustomerId, DateTime DateTime, SaleChannel Channel,
    SaleStatus Status, string? Notes, Guid ClientGuid, List<SaleLineDto> Lines, List<SalePaymentDto> Payments);

/// <summary>WasReplay tells the controller which HTTP status to use (doc 08: a first-time
/// ClientGuid creates and returns 201, a replayed one does nothing new and returns 200 with the
/// same, already-existing sale) without the controller needing its own second lookup.</summary>
public record CreateSaleResult(SaleDto Sale, bool WasReplay);

/// <summary>Reason is optional context for why the sale was refunded - not a Sale column (doc
/// 02's field list has none), so RefundSaleAsync folds it into Sale.Notes rather than dropping it
/// (see DECISIONS.md).</summary>
public record RefundSaleRequest(string? Reason);
