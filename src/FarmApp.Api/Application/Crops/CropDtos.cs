namespace FarmApp.Api.Application.Crops;

public record CropDto(int CropId, string Name, bool IsActive);

public record CreateCropRequest(string Name);

public record UpdateCropRequest(string Name, bool IsActive);
