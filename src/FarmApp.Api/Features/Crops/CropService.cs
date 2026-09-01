using FarmApp.Api.Shared;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Features.Crops;

public class CropService(ICropRepository repo, IUnitOfWork uow) : ICropService
{
    public Task<List<CropDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(c => new CropDto(c.CropId, c.Name, c.IsActive), includeInactive, ct);

    public Task<CropDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, c => new CropDto(c.CropId, c.Name, c.IsActive), ct);

    public async Task<ServiceResult<CropDto>> CreateAsync(CreateCropRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByNameAsync(request.Name, excludeId: null, ct))
            return ServiceResult<CropDto>.Fail(ServiceError.DuplicateName);

        var crop = new Crop { Name = request.Name };
        await repo.AddAsync(crop, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<CropDto>.Ok(new CropDto(crop.CropId, crop.Name, crop.IsActive));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateCropRequest request, CancellationToken ct)
    {
        var crop = await repo.GetByIdAsync(id, ct);
        if (crop is null) return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        crop.Name = request.Name;
        crop.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var crop = await repo.GetByIdAsync(id, ct);
        if (crop is null) return ServiceError.NotFound;

        crop.IsActive = false;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
