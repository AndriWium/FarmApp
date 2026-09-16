namespace FarmApp.Api.Application.Reports;

/// <summary>One SaleLine row from Reporting.SalesAnalysis - detail grain (doc 04 §2). The API/
/// Angular layer aggregates by whichever of product/grade/pack-size/channel/customer/day-of-week
/// the screen needs; the view deliberately doesn't pre-aggregate to one specific grouping.</summary>
public record SalesAnalysisRowDto(
    int SaleLineId, int SaleId, DateTime SaleDateTime, string Channel, int? CustomerId,
    int ProductId, string ProductName, int? GradeId, string? GradeName,
    int? PackSizeId, string? PackSizeName,
    decimal Qty, decimal UnitPrice, decimal DiscountAmount, string? DiscountReason,
    decimal CostAtSale, decimal LineTotal, decimal GrossMargin);

/// <summary>On-hand quantity and value for one Product/Grade combination, from
/// Reporting.StockOnHand.</summary>
public record StockOnHandDto(int ProductId, string ProductName, int? GradeId, string? GradeName, decimal QtyOnHand, decimal Value);

/// <summary>Opening + in − out = closing for one Product/Grade over a date range (doc 04 §3's
/// "movement summary" balance check, doc 10 §1's month-end close checklist item). Balances by
/// construction - OpeningBalance/TotalIn/TotalOut/ClosingBalance are all partitions of the same
/// underlying SUM(Qty), so ClosingBalance always equals OpeningBalance + TotalIn − TotalOut;
/// the point of exposing all four is to make that arithmetic visible/drillable, not to detect a
/// data-integrity bug (a real mismatch here would mean the query itself is wrong).</summary>
public record StockMovementSummaryDto(
    int ProductId, string ProductName, int? GradeId, string? GradeName,
    decimal OpeningBalance, decimal TotalIn, decimal TotalOut, decimal ClosingBalance);

/// <summary>One category's total for the income statement's "Expenses (by category)" line.</summary>
public record ExpenseCategoryTotalDto(int ExpenseCategoryId, string CategoryName, decimal Total);

/// <summary>doc 04 §1's income statement. GrossProfit = Sales − Cos − Wastage (doc 04's own
/// structure: Wastage is subtracted before arriving at gross profit, shown as its own line
/// rather than folded into Cos); NetProfit = GrossProfit − TotalExpenses. Field order matches
/// the task brief's own suggested shape.</summary>
public record IncomeStatementDto(
    decimal Sales, decimal Cos, decimal GrossProfit, decimal Wastage,
    List<ExpenseCategoryTotalDto> ExpensesByCategory, decimal TotalExpenses, decimal NetProfit);

// ---- Farming report (doc 04 §4, Phase 4c) ----

/// <summary>Per block/season: costs to date, kg harvested, cost per kg, revenue attributable and
/// margin (doc 04 §4's "should I plant this again?" table). For a Closed season these figures are
/// the posted true-up (SeasonCostSummary) - IsEstimate false. For an Open season they're computed
/// live via ISeasonCostCalculator from the same accumulator queries SeasonCostingService uses,
/// rather than waiting for close (task brief) - IsEstimate true. RevenueAttributed is always an
/// approximation (see ReportQueries.GetSeasonRevenueAttributedAsync/DECISIONS.md) regardless of
/// season status, since the schema has no exact SaleLine-to-batch link either way.</summary>
public record SeasonFarmingReportDto(
    int SeasonId, string SeasonName, string Status,
    decimal InputCost, decimal LabourCost, decimal OverheadAllocated, decimal TotalCost,
    decimal KgHarvested, decimal CostPerKg,
    decimal RevenueAttributed, decimal Margin, bool IsEstimate);

/// <summary>One product/grade line of doc 04 §4's harvest summary - this season's kg vs. the same
/// crop's most recent earlier season (null when no prior season exists for this crop yet).</summary>
public record HarvestSummaryRowDto(
    int ProductId, string ProductName, int? GradeId, string? GradeName,
    decimal CurrentSeasonKg, decimal? PriorSeasonKg);

/// <summary>One input item's usage/cost for doc 04 §4's input usage summary, over whatever
/// season/date-range filter the caller supplied.</summary>
public record InputUsageRowDto(int InputItemId, string InputItemName, decimal QtyUsed, decimal Cost);

/// <summary>This period's total rainfall vs. the average of the same calendar month across every
/// OTHER year with data (doc 04 §4). HistoricalAverageMm/YearsCompared are null/0 when no other
/// year has any reading for this month yet - a young dataset legitimately has nothing to compare
/// against (task brief: "return null/empty rather than erroring").</summary>
public record RainfallComparisonDto(int Year, int Month, decimal ThisPeriodMm, decimal? HistoricalAverageMm, int YearsCompared);
