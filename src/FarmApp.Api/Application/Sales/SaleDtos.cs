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
