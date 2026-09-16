-- Reporting.ExpensesByCategory
--
-- Detail grain: one row per Expense, joined to ExpenseCategory for its display name and
-- IsFarmingDirect flag. Feeds the income statement's "Expenses (by category)" line (doc 04 §1) -
-- the caller (ReportQueries.GetIncomeStatementAsync) sums Amount grouped by CategoryName over a
-- date range.
CREATE VIEW Reporting.ExpensesByCategory AS
SELECT
    e.ExpenseId,
    e.[Date],
    e.ExpenseCategoryId,
    c.Name              AS CategoryName,
    c.IsFarmingDirect,
    e.Amount,
    e.VatAmount,
    e.SupplierId,
    e.SeasonId
FROM dbo.Expenses e
JOIN dbo.ExpenseCategories c ON c.ExpenseCategoryId = e.ExpenseCategoryId;
