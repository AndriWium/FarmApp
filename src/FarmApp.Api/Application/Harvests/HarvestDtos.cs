namespace FarmApp.Api.Application.Harvests;

/// <summary>UnitCost is included for read-back even though HarvestLine itself doesn't persist it
/// (doc 02's field list) - it's the season-estimate cost that flowed into the line's StockBatch
/// (see DECISIONS.md), sourced from there. StockBatchId is included for traceability, matching
/// ProducePurchaseLineDto's precedent.</summary>
public record HarvestLineDto(
    int HarvestLineId, int ProductId, int? GradeId, decimal QtyKg, decimal UnitCost, int StockBatchId);

public record HarvestDto(
    int HarvestId, int SeasonId, DateTime Date, int? PickedBy, string? Notes,
    string? WithholdingOverrideReason, List<HarvestLineDto> Lines);

/// <summary>One line of a CreateHarvestAsync call. UnitCost and ShelfLifeDays aren't part of doc
/// 02's HarvestLine field list (neither is persisted on the line) - both are supplied here purely
/// to build the line's StockBatch, which requires them (same shape as
/// CreatePurchaseLineRequest.ShelfLifeDays; see DECISIONS.md). UnitCost is the season's *estimate*
/// cost per kg (doc 09) - never a real computed cost at this stage.</summary>
public record CreateHarvestLineRequest(int ProductId, int? GradeId, decimal QtyKg, decimal UnitCost, int ShelfLifeDays);

/// <summary>PickedBy is optional and never defaulted from the authenticated caller (see
/// DECISIONS.md - the person physically picking may not be the person logging the harvest).
/// WithholdingOverrideReason is required only when the harvest date falls inside a withholding
/// lock on this season's block (doc 05 §5) - supplying it when the block isn't locked is
/// harmless and simply ignored (never persisted with a stray, meaningless reason attached).</summary>
public record CreateHarvestRequest(
    int SeasonId, DateTime Date, int? PickedBy, string? Notes, string? WithholdingOverrideReason,
    List<CreateHarvestLineRequest> Lines);
