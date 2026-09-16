using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <summary>Hand-edited (doc 11/db/views convention) - SQL kept in sync with
    /// db/views/Reporting.SalesAnalysis.sql, the versioned source of truth for this view.</summary>
    public partial class AddSalesAnalysisView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW Reporting.SalesAnalysis AS
SELECT
    sl.SaleLineId,
    sl.SaleId,
    s.[DateTime]                                                   AS SaleDateTime,
    s.Channel,
    s.CustomerId,
    sl.ProductId,
    p.Name                                                         AS ProductName,
    sl.GradeId,
    g.Name                                                         AS GradeName,
    sl.PackSizeId,
    ps.Name                                                        AS PackSizeName,
    sl.Qty,
    sl.UnitPrice,
    sl.DiscountAmount,
    sl.DiscountReason,
    sl.CostAtSale,
    (sl.Qty * sl.UnitPrice - sl.DiscountAmount)                    AS LineTotal,
    (sl.Qty * sl.UnitPrice - sl.DiscountAmount) - (sl.CostAtSale * sl.Qty) AS GrossMargin
FROM dbo.SaleLines sl
JOIN dbo.Sales s ON s.SaleId = sl.SaleId
JOIN dbo.Products p ON p.ProductId = sl.ProductId
LEFT JOIN dbo.Grades g ON g.GradeId = sl.GradeId
LEFT JOIN dbo.PackSizes ps ON ps.PackSizeId = sl.PackSizeId
WHERE s.Status <> 'Refunded';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW Reporting.SalesAnalysis;");
        }
    }
}
