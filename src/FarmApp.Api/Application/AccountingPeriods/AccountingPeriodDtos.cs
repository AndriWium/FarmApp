using FarmApp.Api.Application.Reports;

namespace FarmApp.Api.Application.AccountingPeriods;

public record AccountingPeriodDto(
    int AccountingPeriodId, int Year, int Month, string Status,
    DateTime? ClosedAt, string? ClosedBy, DateTime? ReopenedAt, string? ReopenReason);

/// <summary>One TillSession opened within the period that's still open - doc 10 §1's item 1, the
/// only hard-blocking checklist item (task brief: "a month cannot close while a till session for
/// it is still open"). OpenedBy stays the raw AppUser id, matching TillSessionDto's own
/// precedent.</summary>
public record OpenTillSessionRowDto(int TillSessionId, int LocationId, DateTime OpenedAt, int OpenedBy);

/// <summary>One StockTakeLine with a non-zero Variance recorded in the period - doc 10 §1's item
/// 3, implemented as the "more meaningful" interpretation (task brief): surfacing material
/// stock-take variances rather than re-proving the always-true opening+in-out=closing arithmetic
/// identity (see DECISIONS.md).</summary>
public record StockTakeVarianceRowDto(int StockTakeId, DateTime Date, int StockTakeLineId, int StockBatchId, decimal Variance);

/// <summary>One Season flagged for review - either an Open season overlapping the period with no
/// EstimatedCostPerKg yet, or a Closed season whose EndDate falls in the period with no
/// SeasonCostSummary (doc 10 §1's item 4, both halves).</summary>
public record SeasonReviewRowDto(int SeasonId, string Name, string Status, decimal? EstimatedCostPerKg, DateTime EndDate);

/// <summary>doc 10 §1's close checklist, all four applicable items (the outbox item doesn't apply
/// - no offline sync exists). CanClose mirrors TillSessionsAllClosed exactly - it's the only item
/// treated as a hard block (task brief: "unambiguous, per the doc"); every other list here is
/// advisory/informational and never affects CanClose (see DECISIONS.md for the reasoning per
/// item).</summary>
public record CloseChecklistDto(
    int Year, int Month,
    bool TillSessionsAllClosed, IReadOnlyList<OpenTillSessionRowDto> OpenTillSessions,
    IReadOnlyList<StockTakeVarianceRowDto> MaterialStockTakeVariances,
    IReadOnlyList<SeasonReviewRowDto> OpenSeasonsNeedingEstimate,
    IReadOnlyList<SeasonReviewRowDto> ClosedSeasonsMissingTrueUp,
    IReadOnlyList<WastageEntryRowDto> WastageEntriesForReview,
    bool CanClose);

/// <summary>CloseMonthAsync's response - the freshly-closed period AND the checklist that let it
/// close, in one payload (task brief: "return the checklist result alongside the close
/// confirmation so the advisory items are visible in the same response even though they didn't
/// block anything").</summary>
public record CloseMonthResultDto(AccountingPeriodDto Period, CloseChecklistDto Checklist);

/// <summary>Reason is required (doc 10 §1: "reason required") - validated non-empty by
/// ReopenMonthRequestValidator.</summary>
public record ReopenMonthRequest(string Reason);
