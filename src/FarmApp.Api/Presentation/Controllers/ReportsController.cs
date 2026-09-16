using FarmApp.Api.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation.Controllers;

/// <summary>Thin controller straight onto Dapper/raw SQL (doc 11's reporting exception) - no
/// Application service in between, since ReportQueries already returns response-shaped DTOs
/// directly from the Reporting schema's views. Gated behind CanViewReports (Owner only, doc 13's
/// table) - income statement/sales analysis surface margins and COGS.</summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize(Policy = "CanViewReports")]
public class ReportsController(ReportQueries queries) : ControllerBase
{
    [HttpGet("sales-analysis")]
    public async Task<ActionResult<IReadOnlyList<SalesAnalysisRowDto>>> GetSalesAnalysis(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        => Ok(await queries.GetSalesAnalysisAsync(from, to, ct));

    [HttpGet("stock-on-hand")]
    public async Task<ActionResult<IReadOnlyList<StockOnHandDto>>> GetStockOnHand(CancellationToken ct)
        => Ok(await queries.GetStockOnHandAsync(ct));

    [HttpGet("stock-movement-summary")]
    public async Task<ActionResult<IReadOnlyList<StockMovementSummaryDto>>> GetStockMovementSummary(
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] int? productId, [FromQuery] int? gradeId, CancellationToken ct)
        => Ok(await queries.GetStockMovementSummaryAsync(from, to, productId, gradeId, ct));

    [HttpGet("income-statement")]
    public async Task<ActionResult<IncomeStatementDto>> GetIncomeStatement(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        => Ok(await queries.GetIncomeStatementAsync(from, to, ct));
}
