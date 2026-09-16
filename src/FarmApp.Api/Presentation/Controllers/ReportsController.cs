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
public class ReportsController(ReportQueries queries, SeasonFarmingReportService farmingReportService) : ControllerBase
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

    // ---- Farming report (doc 04 §4) ----

    [HttpGet("season-farming/{seasonId:int}")]
    public async Task<ActionResult<SeasonFarmingReportDto>> GetSeasonFarmingReport(int seasonId, CancellationToken ct)
    {
        var dto = await farmingReportService.GetAsync(seasonId, ct);
        return dto is null ? NotFound() : dto;
    }

    [HttpGet("harvest-summary")]
    public async Task<ActionResult<IReadOnlyList<HarvestSummaryRowDto>>> GetHarvestSummary(
        [FromQuery] int seasonId, CancellationToken ct)
        => Ok(await queries.GetHarvestSummaryAsync(seasonId, ct));

    [HttpGet("input-usage")]
    public async Task<ActionResult<IReadOnlyList<InputUsageRowDto>>> GetInputUsage(
        [FromQuery] int? seasonId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await queries.GetInputUsageAsync(seasonId, from, to, ct));

    [HttpGet("rainfall")]
    public async Task<ActionResult<RainfallComparisonDto>> GetRainfall(
        [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
        => Ok(await queries.GetRainfallComparisonAsync(year, month, ct));
}
