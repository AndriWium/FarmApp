using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using FarmApp.Domain.Services;

namespace FarmApp.Api.Application.Reports;

/// <summary>Assembles doc 04 §4's "per block/season" farming report row: costs to date, kg
/// harvested, cost per kg, revenue attributable, margin. Deliberately NOT part of ReportQueries -
/// the cost/kg figures for an OPEN season need the same live accumulator queries and
/// ISeasonCostCalculator arithmetic SeasonCostingService.PreviewCloseAsync already uses (task
/// brief: "don't wait for close"), which means real repository dependencies, not pure Dapper.
/// This is the "light EF query in the Application layer" half of doc 11's reporting exception -
/// it composes that EF-backed cost data with ReportQueries' one Dapper call for the
/// revenue-attribution approximation (see GetSeasonRevenueAttributedAsync), rather than forcing
/// either half into the other's shape (see DECISIONS.md). A plain concrete class, no
/// interface+implementation pair - same reasoning as ReportQueries itself: a report assembler has
/// no second implementation on the horizon.</summary>
public class SeasonFarmingReportService(
    ISeasonRepository seasonRepo,
    ISeasonCostSummaryRepository summaryRepo,
    IActivityInputRepository activityInputRepo,
    IActivityRepository activityRepo,
    IHarvestLineRepository harvestLineRepo,
    ISeasonCostCalculator calculator,
    ReportQueries reportQueries)
{
    public async Task<SeasonFarmingReportDto?> GetAsync(int seasonId, CancellationToken ct)
    {
        var season = await seasonRepo.GetByIdAsync(seasonId, ct);
        if (season is null) return null;

        decimal inputCost, labourCost, totalKg, costPerKg;
        var isEstimate = true;

        if (season.Status == SeasonStatus.Closed)
        {
            var summary = await summaryRepo.GetBySeasonIdAsync(seasonId, ct);
            if (summary is not null)
            {
                inputCost = summary.InputCost;
                labourCost = summary.LabourCost;
                totalKg = summary.TotalKgHarvested;
                costPerKg = summary.CostPerKg;
                isEstimate = false;
            }
            else
            {
                // Shouldn't happen (ISeasonCostingService.ConfirmCloseAsync always creates the
                // summary in the same save that flips Status to Closed) - fall back to a live
                // calculation rather than erroring, same defensive spirit as the close checklist's
                // "flag if somehow missing" item 4 check.
                (inputCost, labourCost, totalKg, costPerKg) = await ComputeLiveAsync(seasonId, ct);
            }
        }
        else
        {
            (inputCost, labourCost, totalKg, costPerKg) = await ComputeLiveAsync(seasonId, ct);
        }

        var revenueAttributed = await reportQueries.GetSeasonRevenueAttributedAsync(seasonId, season.StartDate, season.EndDate, ct);
        var totalCost = inputCost + labourCost; // OverheadAllocated is always 0 (doc 09/11)
        var margin = revenueAttributed - totalCost;

        return new SeasonFarmingReportDto(
            seasonId, season.Name, season.Status.ToString(),
            inputCost, labourCost, 0m, totalCost,
            totalKg, costPerKg, revenueAttributed, margin, isEstimate);
    }

    private async Task<(decimal InputCost, decimal LabourCost, decimal TotalKg, decimal CostPerKg)> ComputeLiveAsync(
        int seasonId, CancellationToken ct)
    {
        var inputCost = await activityInputRepo.GetTotalInputCostForSeasonAsync(seasonId, ct);
        var labourCost = await activityRepo.GetTotalLabourCostForSeasonAsync(seasonId, ct);
        var totalKg = await harvestLineRepo.GetTotalKgHarvestedForSeasonAsync(seasonId, ct);
        var costPerKg = calculator.CalculateCostPerKg(new SeasonAccumulatedCosts(inputCost, labourCost, 0m, totalKg));
        return (inputCost, labourCost, totalKg, costPerKg);
    }
}
