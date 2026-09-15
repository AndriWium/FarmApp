using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.StockBatches;

public record StockBatchDto(
    int StockBatchId, int ProductId, int? GradeId, StockSource Source,
    int? HarvestId, int? PurchaseLineId, int? ProductionBatchId,
    DateTime Date, decimal QtyIn, decimal UnitCost, int ShelfLifeDays, DateTime BestBeforeDate);

/// <summary>Direct batch creation - a stopgap for this phase (see DECISIONS.md). Phase 1b's
/// purchase-in/harvest-in flows will create batches for real through their own use cases; this
/// exists so batches (and their seeding "stock in" movement) can exist at all before then.
/// Source is restricted to Harvest/Purchase here - Production needs StockMovement rows to exist
/// first (the task brief calls this circular), so it isn't creatable through this endpoint.</summary>
public record CreateStockBatchRequest(
    int ProductId, int? GradeId, StockSource Source,
    int? HarvestId, int? PurchaseLineId,
    DateTime Date, decimal QtyIn, decimal UnitCost, int ShelfLifeDays);
