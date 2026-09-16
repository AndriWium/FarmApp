namespace FarmApp.Domain.Repositories;

/// <summary>One StockTakeLine with a non-zero Variance, whose StockTake falls within a given
/// accounting period - the month-end close checklist's stock-take-variance item (doc 10 §1 item
/// 3, task brief's "more meaningful" interpretation: surface material stock-take variances rather
/// than re-proving the always-true opening+in-out=closing arithmetic identity). Date is the
/// StockTake header's own Date (StockTakeLine itself carries no date), included here because the
/// primary aggregate being surfaced is the line/variance, matching
/// IActivityInputRepository.GetChemicalSpraysForBlockAsync's "the repository owns the query whose
/// primary entity it aggregates" precedent.</summary>
public record StockTakeVarianceRow(int StockTakeId, DateTime Date, int StockTakeLineId, int StockBatchId, decimal Variance);
