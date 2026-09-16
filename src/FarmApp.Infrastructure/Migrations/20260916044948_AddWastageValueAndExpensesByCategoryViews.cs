using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <summary>Hand-edited (doc 11/db/views convention) - SQL kept in sync with
    /// db/views/Reporting.WastageValue.sql and db/views/Reporting.ExpensesByCategory.sql, the
    /// versioned sources of truth for these views. Both feed the income-statement endpoint
    /// (ReportQueries.GetIncomeStatementAsync).</summary>
    public partial class AddWastageValueAndExpensesByCategoryViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
");

            migrationBuilder.Sql(@"
CREATE VIEW Reporting.ExpensesByCategory AS
SELECT
    e.ExpenseId,
    e.[Date],
    e.ExpenseCategoryId,
    c.Name              AS CategoryName,
    c.IsFarmingDirect,
    e.Amount,
    e.VatAmount,
    e.SupplierId,
    e.SeasonId
FROM dbo.Expenses e
JOIN dbo.ExpenseCategories c ON c.ExpenseCategoryId = e.ExpenseCategoryId;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW Reporting.ExpensesByCategory;");
            migrationBuilder.Sql("DROP VIEW Reporting.WastageValue;");
        }
    }
}
