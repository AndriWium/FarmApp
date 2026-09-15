namespace FarmApp.Api.Application.ProducePurchases;

/// <summary>StockBatchId is the batch created from this line (doc 02: ProducePurchaseLine
/// "creates StockBatch") - included for traceability, not a persisted column on the line
/// itself (looked up via StockBatch.PurchaseLineId).</summary>
public record ProducePurchaseLineDto(
    int ProducePurchaseLineId, int ProductId, int? GradeId, decimal Qty, decimal UnitCost, int StockBatchId);

public record ProducePurchaseDto(
    int ProducePurchaseId, int SupplierId, DateTime Date, string? InvoiceRef, List<ProducePurchaseLineDto> Lines);

/// <summary>One line of a CreatePurchaseAsync call. ShelfLifeDays isn't part of doc 02's
/// ProducePurchaseLine field list (it's not persisted on the line) - it's supplied here purely
/// to build the line's StockBatch, which requires it (see DECISIONS.md).</summary>
public record CreatePurchaseLineRequest(int ProductId, int? GradeId, decimal Qty, decimal UnitCost, int ShelfLifeDays);

public record CreatePurchaseRequest(int SupplierId, DateTime Date, string? InvoiceRef, List<CreatePurchaseLineRequest> Lines);
