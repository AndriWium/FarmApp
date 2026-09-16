using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonCostSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeasonCostSummaries",
                columns: table => new
                {
                    SeasonCostSummaryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    InputCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LabourCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OverheadAllocated = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalKgHarvested = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CostPerKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TrueUpAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonCostSummaries", x => x.SeasonCostSummaryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonCostSummaries_SeasonId",
                table: "SeasonCostSummaries",
                column: "SeasonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeasonCostSummaries");
        }
    }
}
