namespace FarmApp.Api.Application.Cultivars;

public record CultivarDto(int CultivarId, int CropId, string Name, bool IsActive);

public record CreateCultivarRequest(int CropId, string Name);

public record UpdateCultivarRequest(int CropId, string Name, bool IsActive);
