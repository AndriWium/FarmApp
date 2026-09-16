using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonCostingEstimateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCostPerKg",
                table: "Seasons",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedTotalCost",
                table: "Seasons",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedYieldKg",
                table: "Seasons",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Seasons",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Open");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedCostPerKg",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "ExpectedTotalCost",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "ExpectedYieldKg",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Seasons");
        }
    }
}
