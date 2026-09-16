using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmApp.Infrastructure.Migrations
{
    /// <summary>Hand-edited migration (doc 11: "reports bypass repositories deliberately... via
    /// the Reporting schema views") - no EF model changed, so `ef migrations add` generated an
    /// empty Up/Down; the raw SQL below creates the schema every report view (Phase 4b onward)
    /// lives in. SQL Server requires a schema to be empty before it can be dropped, so every
    /// later migration that adds a Reporting view must drop that view in ITS OWN Down() before
    /// this migration's Down() ever runs (migrations revert in reverse chronological order, so
    /// that ordering falls out naturally as long as no later Down() is skipped).</summary>
    public partial class AddReportingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE SCHEMA Reporting;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SCHEMA Reporting;");
        }
    }
}
