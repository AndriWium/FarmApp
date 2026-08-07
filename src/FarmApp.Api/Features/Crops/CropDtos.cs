namespace FarmApp.Api.Features.Crops;

public record CropDto(
    int CropId,
    string Name
    );

public record CreateCropRequest(string Name);
