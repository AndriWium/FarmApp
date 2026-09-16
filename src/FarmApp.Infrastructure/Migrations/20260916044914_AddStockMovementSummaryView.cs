using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <summary>Hand-edited (doc 11/db/views convention) - SQL kept in sync with
    /// db/views/Reporting.StockMovementSummary.sql, the versioned source of truth for this
    /// view.</summary>
    public partial class AddStockMovementSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW Reporting.StockMovementSummary;");
        }
    }
}
