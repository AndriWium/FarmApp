namespace FarmApp.Api.Application.ActivityTypes;

public record ActivityTypeDto(int ActivityTypeId, string Name, string Category, bool IsActive);

public record CreateActivityTypeRequest(string Name, string Category);

public record UpdateActivityTypeRequest(string Name, string Category, bool IsActive);
