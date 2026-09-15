using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Locations;

public class LocationService(ILocationRepository repo, IUnitOfWork uow) : ILocationService
{
    public Task<List<LocationDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(l => new LocationDto(l.LocationId, l.Name, l.IsActive), includeInactive, ct);

    public Task<LocationDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, l => new LocationDto(l.LocationId, l.Name, l.IsActive), ct);

    public async Task<ServiceResult<LocationDto>> CreateAsync(CreateLocationRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByNameAsync(request.Name, excludeId: null, ct))
            return ServiceResult<LocationDto>.Fail(ServiceError.DuplicateName);

        var location = new Location { Name = request.Name };
        await repo.AddAsync(location, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<LocationDto>.Ok(new LocationDto(location.LocationId, location.Name, location.IsActive));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateLocationRequest request, CancellationToken ct)
    {
        var location = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (location is null) return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        location.Name = request.Name;
        location.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var location = await repo.GetByIdAsync(id, ct);
        if (location is null) return ServiceError.NotFound;

        location.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
