namespace FarmApp.Api.Application.Locations;

public record LocationDto(int LocationId, string Name, bool IsActive);

public record CreateLocationRequest(string Name);

public record UpdateLocationRequest(string Name, bool IsActive);
