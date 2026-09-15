using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.ActivityTypes;

public class ActivityTypeService(IActivityTypeRepository repo, IUnitOfWork uow) : IActivityTypeService
{
    public Task<List<ActivityTypeDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(x => new ActivityTypeDto(x.ActivityTypeId, x.Name, x.Category, x.IsActive), includeInactive, ct);

    public Task<ActivityTypeDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, x => new ActivityTypeDto(x.ActivityTypeId, x.Name, x.Category, x.IsActive), ct);

    public async Task<ServiceResult<ActivityTypeDto>> CreateAsync(CreateActivityTypeRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByNameAsync(request.Name, excludeId: null, ct))
            return ServiceResult<ActivityTypeDto>.Fail(ServiceError.DuplicateName);

        var activityType = new ActivityType { Name = request.Name, Category = request.Category };
        await repo.AddAsync(activityType, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<ActivityTypeDto>.Ok(
            new ActivityTypeDto(activityType.ActivityTypeId, activityType.Name, activityType.Category, activityType.IsActive));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateActivityTypeRequest request, CancellationToken ct)
    {
        var activityType = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (activityType is null) return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        activityType.Name = request.Name;
        activityType.Category = request.Category;
        activityType.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var activityType = await repo.GetByIdAsync(id, ct);
        if (activityType is null) return ServiceError.NotFound;

        activityType.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
