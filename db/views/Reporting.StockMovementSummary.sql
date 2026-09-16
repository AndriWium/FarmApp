-- Reporting.StockMovementSummary
--
-- Detail grain: one row per StockMovement, joined to Product/Grade for display names. A SQL
-- Server view cannot take parameters, so the opening/in/out/closing balance-check arithmetic
-- (doc 04 §3, doc 10 §1's month-end checklist item) is computed by the caller
-- (ReportQueries.GetStockMovementSummaryAsync) with a date-range-parameterised aggregate query
-- against this view, not baked into the view itself.
CREATE VIEW Reporting.StockMovementSummary AS
SELECT
    m.StockMovementId,
    m.StockBatchId,
    b.ProductId,
    p.Name       AS ProductName,
    b.GradeId,
    g.Name       AS GradeName,
    m.[Date],
    m.[Type],
    m.Qty
FROM dbo.StockMovements m
JOIN dbo.StockBatches b ON b.StockBatchId = m.StockBatchId
JOIN dbo.Products p ON p.ProductId = b.ProductId
LEFT JOIN dbo.Grades g ON g.GradeId = b.GradeId;
