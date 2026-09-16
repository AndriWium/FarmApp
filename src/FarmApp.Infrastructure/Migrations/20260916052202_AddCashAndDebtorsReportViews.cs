using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <summary>Hand-edited (doc 11/db/views convention) - SQL kept in sync with the four
    /// db/views/Reporting.*.sql files this migration adds (Phase 4c's cash & debtors report,
    /// doc 04 §5).</summary>
    public partial class AddCashAndDebtorsReportViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW Reporting.TillSessionsOverShort AS
SELECT
    t.TillSessionId,
    t.LocationId,
    t.OpenedAt,
    t.OpenedBy,
    t.ClosedAt,
    t.SystemCardTotal,
    t.CardMachineBatchTotal,
    t.Difference,
    t.DifferenceNote
FROM dbo.TillSessions t
WHERE t.ClosedAt IS NOT NULL;
");

            migrationBuilder.Sql(@"
CREATE VIEW Reporting.SalePaymentDetail AS
SELECT
    sp.SalePaymentId,
    s.[DateTime] AS SaleDateTime,
    sp.Method,
    sp.Amount
FROM dbo.SalePayments sp
JOIN dbo.Sales s ON s.SaleId = sp.SaleId
WHERE s.Status <> 'Refunded';
");

            migrationBuilder.Sql(@"
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
");

            migrationBuilder.Sql(@"
CREATE VIEW Reporting.AccountSalesDetail AS
SELECT
    s.CustomerId,
    s.SaleId,
    s.[DateTime] AS SaleDate,
    sp.Amount
FROM dbo.Sales s
JOIN dbo.SalePayments sp ON sp.SaleId = s.SaleId AND sp.Method = 'Account'
WHERE s.Status <> 'Refunded' AND s.CustomerId IS NOT NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW Reporting.AccountSalesDetail;");
            migrationBuilder.Sql("DROP VIEW Reporting.PurchaseCosts;");
            migrationBuilder.Sql("DROP VIEW Reporting.SalePaymentDetail;");
            migrationBuilder.Sql("DROP VIEW Reporting.TillSessionsOverShort;");
        }
    }
}
