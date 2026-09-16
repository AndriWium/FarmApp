-- Reporting.StockOnHand
--
-- On-hand quantity and value per Product/Grade, exactly the aggregation
-- StockMovementRepository.GetOnHandSummaryAsync currently does client-side (materialize-then-
-- group in memory, per Phase 1a's DECISIONS.md - EF Core 10 couldn't translate the multi-table
-- GroupBy server-side). This view is the "real" reporting mechanism that entry anticipated.
CREATE VIEW Reporting.StockOnHand AS
SELECT
    b.ProductId,
    p.Name                    AS ProductName,
    b.GradeId,
    g.Name                    AS GradeName,
    SUM(m.Qty)                AS QtyOnHand,
    SUM(m.Qty * b.UnitCost)   AS Value
FROM dbo.StockMovements m
JOIN dbo.StockBatches b ON b.StockBatchId = m.StockBatchId
JOIN dbo.Products p ON p.ProductId = b.ProductId
LEFT JOIN dbo.Grades g ON g.GradeId = b.GradeId
GROUP BY b.ProductId, p.Name, b.GradeId, g.Name
HAVING SUM(m.Qty) <> 0;
