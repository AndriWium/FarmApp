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
