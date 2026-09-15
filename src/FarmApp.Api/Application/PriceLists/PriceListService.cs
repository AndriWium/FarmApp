using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.PriceLists;

public class PriceListService(IPriceListRepository repo, IUnitOfWork uow) : IPriceListService
{
    public Task<List<PriceListDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(p => new PriceListDto(p.PriceListId, p.Name, p.IsActive), includeInactive, ct);

    public Task<PriceListDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, p => new PriceListDto(p.PriceListId, p.Name, p.IsActive), ct);

    public async Task<ServiceResult<PriceListDto>> CreateAsync(CreatePriceListRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByNameAsync(request.Name, excludeId: null, ct))
            return ServiceResult<PriceListDto>.Fail(ServiceError.DuplicateName);

        var priceList = new PriceList { Name = request.Name };
        await repo.AddAsync(priceList, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<PriceListDto>.Ok(new PriceListDto(priceList.PriceListId, priceList.Name, priceList.IsActive));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdatePriceListRequest request, CancellationToken ct)
    {
        var priceList = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (priceList is null) return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        priceList.Name = request.Name;
        priceList.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var priceList = await repo.GetByIdAsync(id, ct);
        if (priceList is null) return ServiceError.NotFound;

        priceList.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
