using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <summary>Hand-edited (doc 11/db/views convention) - SQL kept in sync with
    /// db/views/Reporting.StockOnHand.sql, the versioned source of truth for this view.</summary>
    public partial class AddStockOnHandView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW Reporting.StockOnHand;");
        }
    }
}
