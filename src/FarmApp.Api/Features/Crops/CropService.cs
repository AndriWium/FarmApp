using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Features.Crops;

public class CropService(ICropRepository repo, IUnitOfWork uow) : ICropService
{
    public Task<List<CropDto>> GetAllAsync(CancellationToken ct)
        => repo.GetAllAsync(c => new CropDto(c.CropId, c.Name), ct);

    public Task<CropDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, c => new CropDto(c.CropId, c.Name), ct);

    public async Task<CropDto> CreateAsync(CreateCropRequest request, CancellationToken ct)
    {
        var crop = new Crop { Name = request.Name };
        await repo.AddAsync(crop, ct);
        await uow.SaveChangesAsync(ct);
        return new CropDto(crop.CropId, crop.Name);
    }

    public async Task<bool> UpdateAsync(int id, CreateCropRequest request, CancellationToken ct)
    {
        var crop = await repo.GetByIdAsync(id, ct);
        if (crop is null) return false;

        crop.Name = request.Name;
        await uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var crop = await repo.GetByIdAsync(id, ct);
        if (crop is null) return false;

        repo.Remove(crop);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
