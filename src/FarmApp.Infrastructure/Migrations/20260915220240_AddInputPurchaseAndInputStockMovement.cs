using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInputPurchaseAndInputStockMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InputPurchaseLines",
                columns: table => new
                {
                    InputPurchaseLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InputPurchaseId = table.Column<int>(type: "int", nullable: false),
                    InputItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputPurchaseLines", x => x.InputPurchaseLineId);
                });

            migrationBuilder.CreateTable(
                name: "InputPurchases",
                columns: table => new
                {
                    InputPurchaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputPurchases", x => x.InputPurchaseId);
                });

            migrationBuilder.CreateTable(
                name: "InputStockMovements",
                columns: table => new
                {
                    InputStockMovementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InputItemId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RefTable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputStockMovements", x => x.InputStockMovementId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InputPurchaseLines_InputItemId",
                table: "InputPurchaseLines",
                column: "InputItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InputPurchaseLines_InputPurchaseId",
                table: "InputPurchaseLines",
                column: "InputPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_InputPurchases_SupplierId",
                table: "InputPurchases",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_InputStockMovements_InputItemId",
                table: "InputStockMovements",
                column: "InputItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InputPurchaseLines");

            migrationBuilder.DropTable(
                name: "InputPurchases");

            migrationBuilder.DropTable(
                name: "InputStockMovements");
        }
    }
}
