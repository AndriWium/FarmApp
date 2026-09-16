using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadmapItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoadmapItems",
                columns: table => new
                {
                    RoadmapItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TargetPhase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapItems", x => x.RoadmapItemId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapItems_SortOrder",
                table: "RoadmapItems",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadmapItems");
        }
    }
}
