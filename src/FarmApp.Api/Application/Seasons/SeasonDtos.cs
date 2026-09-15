namespace FarmApp.Api.Application.Seasons;

public record SeasonDto(int SeasonId, int PlantingId, string Name, DateTime StartDate, DateTime EndDate);

public record CreateSeasonRequest(int PlantingId, string Name, DateTime StartDate, DateTime EndDate);

public record UpdateSeasonRequest(int PlantingId, string Name, DateTime StartDate, DateTime EndDate);
