-- Reporting.WastageValue
--
-- Feeds the income statement's Wastage line (doc 04 §1: "StockMovement type Wastage x batch
-- cost"). Only Type = 'Wastage' - doc 04 names this one type specifically for the income
-- statement's own wastage line (OwnUse/Sample/Donation are different, deliberate uses of stock,
-- not shrinkage/loss, and are left out of this figure - see DECISIONS.md). Qty is negative for
-- an outflow movement; Value is the absolute cost impact.
CREATE VIEW Reporting.WastageValue AS
SELECT
    m.StockMovementId,
    m.[Date],
    b.ProductId,
    b.GradeId,
    m.Qty,
    b.UnitCost,
    ABS(m.Qty) * b.UnitCost AS Value
FROM dbo.StockMovements m
JOIN dbo.StockBatches b ON b.StockBatchId = m.StockBatchId
WHERE m.[Type] = 'Wastage';
