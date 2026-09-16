namespace FarmApp.Api.Application.SeasonCosting;

/// <summary>Read-only "what would closing this season do right now" preview (doc 09) - callable
/// any number of times while the season is open, writes nothing. EstimatedCostPerKg is the
/// season's own current estimate (Season.EstimatedCostPerKg) at the moment of preview, shown for
/// comparison against ActualCostPerKg - "the estimate it's replacing" in the everyday sense, even
/// though TrueUpAmount itself is computed per batch and so isn't literally
/// (ActualCostPerKg - EstimatedCostPerKg) x TotalKgHarvested whenever the estimate changed
/// mid-season (see DECISIONS.md). Null when the season never had an estimate set at all.</summary>
public record SeasonCostPreviewDto(
    int SeasonId, decimal InputCost, decimal LabourCost, decimal OverheadAllocated, decimal TotalKgHarvested,
    decimal? EstimatedCostPerKg, decimal ActualCostPerKg, decimal TrueUpAmount);

/// <summary>The persisted, permanent true-up record (doc 09/02's SeasonCostSummary) - what
/// ConfirmCloseAsync writes and also returns, so the response to POST .../close *is* the
/// confirmation record.</summary>
public record SeasonCostSummaryDto(
    int SeasonCostSummaryId, int SeasonId, decimal InputCost, decimal LabourCost, decimal OverheadAllocated,
    decimal TotalKgHarvested, decimal CostPerKg, decimal TrueUpAmount, DateTime ClosedAt);
