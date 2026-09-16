namespace FarmApp.Api.Application.Harvests;

/// <summary>UnitCost is included for read-back even though HarvestLine itself doesn't persist it
/// (doc 02's field list) - it's the season-estimate cost that flowed into the line's StockBatch,
/// sourced from there. As of Phase 4a this is always Season.EstimatedCostPerKg at the moment the
/// harvest was recorded (see DECISIONS.md) - never a caller-typed number. StockBatchId is
/// included for traceability, matching ProducePurchaseLineDto's precedent.</summary>
public record HarvestLineDto(
    int HarvestLineId, int ProductId, int? GradeId, decimal QtyKg, decimal UnitCost, int StockBatchId);

public record HarvestDto(
    int HarvestId, int SeasonId, DateTime Date, int? PickedBy, string? Notes,
    string? WithholdingOverrideReason, List<HarvestLineDto> Lines);

/// <summary>One line of a CreateHarvestAsync call. ShelfLifeDays isn't part of doc 02's
/// HarvestLine field list (not persisted on the line) - it's supplied here purely to build the
/// line's StockBatch, which requires it (same shape as CreatePurchaseLineRequest.ShelfLifeDays;
/// see DECISIONS.md). There is deliberately no UnitCost field here (Phase 4a, see DECISIONS.md):
/// the batch's cost is always Season.EstimatedCostPerKg at the moment of harvest (doc 09 - "the
/// season-estimate cost... never a real computed cost"), snapshotted server-side, never accepted
/// from the caller - a season with no estimate set yet rejects the harvest outright
/// (ServiceError.SeasonEstimateNotSet) rather than silently falling back to some arbitrary
/// caller-supplied number.</summary>
public record CreateHarvestLineRequest(int ProductId, int? GradeId, decimal QtyKg, int ShelfLifeDays);

/// <summary>PickedBy is optional and never defaulted from the authenticated caller (see
/// DECISIONS.md - the person physically picking may not be the person logging the harvest).
/// WithholdingOverrideReason is required only when the harvest date falls inside a withholding
/// lock on this season's block (doc 05 §5) - supplying it when the block isn't locked is
/// harmless and simply ignored (never persisted with a stray, meaningless reason attached).</summary>
public record CreateHarvestRequest(
    int SeasonId, DateTime Date, int? PickedBy, string? Notes, string? WithholdingOverrideReason,
    List<CreateHarvestLineRequest> Lines);
