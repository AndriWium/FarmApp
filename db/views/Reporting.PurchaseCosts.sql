-- Reporting.PurchaseCosts
--
-- Detail grain: one row per ProducePurchaseLine/InputPurchaseLine (UNION ALL - same Date/Amount
-- shape, different Kind), for doc 04 §5's cash flow "cash out (expenses, purchases)". Each
-- source's own header carries the Date (neither line entity has one of its own), so both need
-- the join up to their header before this report can filter/sum by date range.
CREATE VIEW Reporting.PurchaseCosts AS
SELECT
    'Produce' AS Kind,
    pp.[Date],
    ppl.Qty * ppl.UnitCost AS Amount
FROM dbo.ProducePurchaseLines ppl
JOIN dbo.ProducePurchases pp ON pp.ProducePurchaseId = ppl.ProducePurchaseId
UNION ALL
SELECT
    'Input' AS Kind,
    ip.[Date],
    ipl.Qty * ipl.UnitCost AS Amount
FROM dbo.InputPurchaseLines ipl
JOIN dbo.InputPurchases ip ON ip.InputPurchaseId = ipl.InputPurchaseId;
