using FarmApp.Domain.Entities;

namespace FarmApp.Api.Application.Seasons;

/// <summary>EstimatedCostPerKg is always server-computed (SeasonService, via
/// ISeasonCostCalculator) from ExpectedTotalCost/ExpectedYieldKg - never accepted directly from a
/// client on create/update, so it never appears on CreateSeasonRequest/UpdateSeasonRequest, only
/// here on the read-back DTO.</summary>
public record SeasonDto(
    int SeasonId, int PlantingId, string Name, DateTime StartDate, DateTime EndDate, SeasonStatus Status,
    decimal? ExpectedTotalCost, decimal? ExpectedYieldKg, decimal? EstimatedCostPerKg);

/// <summary>ExpectedTotalCost/ExpectedYieldKg are both optional (doc 09: the estimate "can start
/// crude... and improve as the season progresses") - a season may legitimately be created with
/// neither set yet, though HarvestService then rejects any harvest against it until both are
/// supplied via UpdateAsync.</summary>
public record CreateSeasonRequest(
    int PlantingId, string Name, DateTime StartDate, DateTime EndDate,
    decimal? ExpectedTotalCost, decimal? ExpectedYieldKg);

/// <summary>Rejected once the season's Status is Closed (doc 09: "editable while the season is
/// open") - see SeasonService.UpdateAsync.</summary>
public record UpdateSeasonRequest(
    int PlantingId, string Name, DateTime StartDate, DateTime EndDate,
    decimal? ExpectedTotalCost, decimal? ExpectedYieldKg);
