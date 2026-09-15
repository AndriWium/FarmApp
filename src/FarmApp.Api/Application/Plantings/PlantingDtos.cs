using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.Plantings;

public record PlantingDto(
    int PlantingId, int BlockId, int CultivarId, DateTime StartDate, DateTime? EndDate,
    PlantingType Type, int? PlantCount, string? Notes);

public record CreatePlantingRequest(
    int BlockId, int CultivarId, DateTime StartDate, DateTime? EndDate,
    PlantingType Type, int? PlantCount, string? Notes);

public record UpdatePlantingRequest(
    int BlockId, int CultivarId, DateTime StartDate, DateTime? EndDate,
    PlantingType Type, int? PlantCount, string? Notes);
