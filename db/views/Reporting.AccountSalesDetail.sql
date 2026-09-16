-- Reporting.AccountSalesDetail
--
-- Detail grain: one row per Account-method SalePayment, joined up to its Sale for CustomerId/Date
-- - the "debts" doc 04 §5's debtors aging bucketing needs (task brief: apply a customer's total
-- payments against their Account-method sales oldest-first). Excludes Refunded sales (a refunded
-- sale is no longer a debt the customer owes) and anonymous sales (CustomerId IS NOT NULL - an
-- Account payment always has a customer, enforced at write time by
-- ServiceError.AccountPaymentRequiresCustomer, but the filter is kept here anyway for clarity/
-- defence). FarmApp.Domain.Services.IDebtorsAgingCalculator consumes this as DebtLine rows,
-- ordered oldest-first by the caller (ReportQueries).
CREATE VIEW Reporting.AccountSalesDetail AS
SELECT
    s.CustomerId,
    s.SaleId,
    s.[DateTime] AS SaleDate,
    sp.Amount
FROM dbo.Sales s
JOIN dbo.SalePayments sp ON sp.SaleId = s.SaleId AND sp.Method = 'Account'
WHERE s.Status <> 'Refunded' AND s.CustomerId IS NOT NULL;
