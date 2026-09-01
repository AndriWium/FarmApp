namespace FarmApp.Api.Features.Crops;

public interface ICropService
{
    Task<List<CropDto>> GetAllAsync(CancellationToken ct);
    Task<CropDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CropDto> CreateAsync(CreateCropRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, CreateCropRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
