-- Reporting.SalePaymentDetail
--
-- Detail grain: one row per SalePayment, with its Sale's date for date-range filtering - the
-- "cash in" (really card/EFT-in, doc 01 §4's card-only decision) half of doc 04 §5's cash flow
-- summary. Excludes Refunded sales, matching Reporting.SalesAnalysis's own filter - a refunded
-- sale's original payment row is a historical fact but shouldn't count as money that stayed in
-- the business for a cash-flow-over-a-period report.
CREATE VIEW Reporting.SalePaymentDetail AS
SELECT
    sp.SalePaymentId,
    s.[DateTime] AS SaleDateTime,
    sp.Method,
    sp.Amount
FROM dbo.SalePayments sp
JOIN dbo.Sales s ON s.SaleId = sp.SaleId
WHERE s.Status <> 'Refunded';
