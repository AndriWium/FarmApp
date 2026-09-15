using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Cultivars;

public class CultivarService(ICultivarRepository repo, ICropRepository cropRepo, IUnitOfWork uow) : ICultivarService
{
    public Task<List<CultivarDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(c => new CultivarDto(c.CultivarId, c.CropId, c.Name, c.IsActive), includeInactive, ct);

    public Task<CultivarDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, c => new CultivarDto(c.CultivarId, c.CropId, c.Name, c.IsActive), ct);

    public async Task<ServiceResult<CultivarDto>> CreateAsync(CreateCultivarRequest request, CancellationToken ct)
    {
        if (await cropRepo.GetByIdAsync(request.CropId, ct) is null)
            return ServiceResult<CultivarDto>.Fail(ServiceError.NotFound);

        if (await repo.ExistsByNameAsync(request.CropId, request.Name, excludeId: null, ct))
            return ServiceResult<CultivarDto>.Fail(ServiceError.DuplicateName);

        var cultivar = new Cultivar { CropId = request.CropId, Name = request.Name };
        await repo.AddAsync(cultivar, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<CultivarDto>.Ok(
            new CultivarDto(cultivar.CultivarId, cultivar.CropId, cultivar.Name, cultivar.IsActive));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateCultivarRequest request, CancellationToken ct)
    {
        var cultivar = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (cultivar is null) return ServiceError.NotFound;

        if (await cropRepo.GetByIdAsync(request.CropId, ct) is null)
            return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.CropId, request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        cultivar.CropId = request.CropId;
        cultivar.Name = request.Name;
        cultivar.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var cultivar = await repo.GetByIdAsync(id, ct);
        if (cultivar is null) return ServiceError.NotFound;

        cultivar.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
