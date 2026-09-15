namespace FarmApp.Api.Application.RainfallLogs;

public record RainfallLogDto(int RainfallLogId, DateOnly Date, decimal Mm, string? Notes);

public record CreateRainfallLogRequest(DateOnly Date, decimal Mm, string? Notes);

public record UpdateRainfallLogRequest(decimal Mm, string? Notes);
