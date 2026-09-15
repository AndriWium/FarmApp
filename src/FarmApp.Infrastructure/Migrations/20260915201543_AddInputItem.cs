using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInputItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InputItems",
                columns: table => new
                {
                    InputItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReorderLevel = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WithholdingDays = table.Column<int>(type: "int", nullable: true),
                    ActiveIngredient = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputItems", x => x.InputItemId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InputItems_Name",
                table: "InputItems",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InputItems");
        }
    }
}
