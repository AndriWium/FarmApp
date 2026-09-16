using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <summary>Hand-edited (doc 11/db/views convention) - SQL kept in sync with the three
    /// db/views/Reporting.*.sql files this migration adds (Phase 4c's farming report, doc 04 §4).</summary>
    public partial class AddFarmingReportViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW Reporting.HarvestSummaryBySeason AS
SELECT
    s.SeasonId,
    s.Name          AS SeasonName,
    s.StartDate,
    s.EndDate,
    cr.CropId,
    cr.Name         AS CropName,
    hl.ProductId,
    p.Name          AS ProductName,
    hl.GradeId,
    g.Name          AS GradeName,
    SUM(hl.QtyKg)   AS QtyKg
FROM dbo.Seasons s
JOIN dbo.Plantings pl ON pl.PlantingId = s.PlantingId
JOIN dbo.Cultivars cv ON cv.CultivarId = pl.CultivarId
JOIN dbo.Crops cr ON cr.CropId = cv.CropId
JOIN dbo.Harvests h ON h.SeasonId = s.SeasonId
JOIN dbo.HarvestLines hl ON hl.HarvestId = h.HarvestId
JOIN dbo.Products p ON p.ProductId = hl.ProductId
LEFT JOIN dbo.Grades g ON g.GradeId = hl.GradeId
GROUP BY s.SeasonId, s.Name, s.StartDate, s.EndDate, cr.CropId, cr.Name, hl.ProductId, p.Name, hl.GradeId, g.Name;
");

            migrationBuilder.Sql(@"
CREATE VIEW Reporting.InputUsageBySeason AS
SELECT
    ai.ActivityInputId,
    a.SeasonId,
    a.[Date],
    ai.InputItemId,
    ii.Name             AS InputItemName,
    ai.Qty,
    ai.UnitCost,
    ai.Qty * ai.UnitCost AS Cost
FROM dbo.ActivityInputs ai
JOIN dbo.Activities a ON a.ActivityId = ai.ActivityId
JOIN dbo.InputItems ii ON ii.InputItemId = ai.InputItemId;
");

            migrationBuilder.Sql(@"
CREATE VIEW Reporting.SaleOutDepletion AS
SELECT
    m.StockMovementId,
    m.[Date],
    b.ProductId,
    b.GradeId,
    -m.Qty      AS QtyDepleted,
    h.SeasonId  AS SeasonId
FROM dbo.StockMovements m
JOIN dbo.StockBatches b ON b.StockBatchId = m.StockBatchId
JOIN dbo.Sales s ON s.SaleId = m.RefId AND m.RefTable = 'Sale'
LEFT JOIN dbo.Harvests h ON h.HarvestId = b.HarvestId
WHERE m.[Type] = 'SaleOut' AND s.Status <> 'Refunded';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW Reporting.SaleOutDepletion;");
            migrationBuilder.Sql("DROP VIEW Reporting.InputUsageBySeason;");
            migrationBuilder.Sql("DROP VIEW Reporting.HarvestSummaryBySeason;");
        }
    }
}
