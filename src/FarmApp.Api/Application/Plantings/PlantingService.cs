using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Plantings;

public class PlantingService(
    IPlantingRepository repo, IBlockRepository blockRepo, ICultivarRepository cultivarRepo, IUnitOfWork uow)
    : IPlantingService
{
    public Task<List<PlantingDto>> GetAllAsync(int? blockId, CancellationToken ct)
        => repo.GetAllAsync(ToDtoExpr(), blockId, ct);

    public Task<PlantingDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDtoExpr(), ct);

    public async Task<ServiceResult<PlantingDto>> CreateAsync(CreatePlantingRequest request, CancellationToken ct)
    {
        if (await blockRepo.GetByIdAsync(request.BlockId, ct) is null)
            return ServiceResult<PlantingDto>.Fail(ServiceError.NotFound);

        if (await cultivarRepo.GetByIdAsync(request.CultivarId, ct) is null)
            return ServiceResult<PlantingDto>.Fail(ServiceError.NotFound);

        var planting = new Planting
        {
            BlockId = request.BlockId,
            CultivarId = request.CultivarId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Type = request.Type,
            PlantCount = request.PlantCount,
            Notes = request.Notes,
        };
        await repo.AddAsync(planting, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<PlantingDto>.Ok(ToDto(planting));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdatePlantingRequest request, CancellationToken ct)
    {
        var planting = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (planting is null) return ServiceError.NotFound;

        if (await blockRepo.GetByIdAsync(request.BlockId, ct) is null)
            return ServiceError.NotFound;

        if (await cultivarRepo.GetByIdAsync(request.CultivarId, ct) is null)
            return ServiceError.NotFound;

        planting.BlockId = request.BlockId;
        planting.CultivarId = request.CultivarId;
        planting.StartDate = request.StartDate;
        planting.EndDate = request.EndDate;
        planting.Type = request.Type;
        planting.PlantCount = request.PlantCount;
        planting.Notes = request.Notes;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    private static System.Linq.Expressions.Expression<Func<Planting, PlantingDto>> ToDtoExpr()
        => p => new PlantingDto(p.PlantingId, p.BlockId, p.CultivarId, p.StartDate, p.EndDate, p.Type, p.PlantCount, p.Notes);

    private static PlantingDto ToDto(Planting p)
        => new(p.PlantingId, p.BlockId, p.CultivarId, p.StartDate, p.EndDate, p.Type, p.PlantCount, p.Notes);
}
