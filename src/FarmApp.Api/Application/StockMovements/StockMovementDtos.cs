using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.StockMovements;

public record StockMovementDto(
    int StockMovementId, int StockBatchId, DateTime Date, StockMovementType Type,
    decimal Qty, string? RefTable, int? RefId, string? Reason, int? LocationId);

/// <summary>Shared request shape for the FIFO-depleted, single-location movement types
/// (Wastage, OwnUse, Sample, Donation, Adjustment, Repack). Qty is always entered as a positive
/// number — the service decides the sign and which batch(es) it comes from.</summary>
public record RecordStockMovementRequest(int ProductId, int? GradeId, decimal Qty, string? Reason, int? LocationId);

/// <summary>"Move 5kg from Farm store to Market stall" — one user action producing a paired
/// TransferOut/TransferIn movement (per batch touched) for the same quantity.</summary>
public record TransferStockRequest(int ProductId, int? GradeId, decimal Qty, int FromLocationId, int ToLocationId, string? Reason);

public record StockOnHandSummaryDto(int ProductId, string ProductName, int? GradeId, string? GradeName, decimal QtyOnHand, decimal Value);
