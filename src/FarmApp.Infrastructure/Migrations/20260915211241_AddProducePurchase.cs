using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProducePurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProducePurchaseLines",
                columns: table => new
                {
                    ProducePurchaseLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProducePurchaseId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducePurchaseLines", x => x.ProducePurchaseLineId);
                });

            migrationBuilder.CreateTable(
                name: "ProducePurchases",
                columns: table => new
                {
                    ProducePurchaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducePurchases", x => x.ProducePurchaseId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProducePurchaseLines_ProducePurchaseId",
                table: "ProducePurchaseLines",
                column: "ProducePurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducePurchases_SupplierId",
                table: "ProducePurchases",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProducePurchaseLines");

            migrationBuilder.DropTable(
                name: "ProducePurchases");
        }
    }
}
