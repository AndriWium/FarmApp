using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.PackSizes;

public class PackSizeService(IPackSizeRepository repo, IProductRepository productRepo, IUnitOfWork uow) : IPackSizeService
{
    public Task<List<PackSizeDto>> GetAllAsync(CancellationToken ct)
        => repo.GetAllAsync(ToDtoExpr, ct);

    public Task<PackSizeDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDtoExpr, ct);

    public async Task<ServiceResult<PackSizeDto>> CreateAsync(CreatePackSizeRequest request, CancellationToken ct)
    {
        if (await productRepo.GetByIdAsync(request.ProductId, ct) is null)
            return ServiceResult<PackSizeDto>.Fail(ServiceError.NotFound);

        if (await repo.ExistsByNameAsync(request.ProductId, request.Name, excludeId: null, ct))
            return ServiceResult<PackSizeDto>.Fail(ServiceError.DuplicateName);

        var packSize = new PackSize
        {
            ProductId = request.ProductId,
            Name = request.Name,
            QtyInBaseUnit = request.QtyInBaseUnit,
        };
        await repo.AddAsync(packSize, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<PackSizeDto>.Ok(ToDto(packSize));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdatePackSizeRequest request, CancellationToken ct)
    {
        var packSize = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (packSize is null) return ServiceError.NotFound;

        if (await productRepo.GetByIdAsync(request.ProductId, ct) is null)
            return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.ProductId, request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        packSize.ProductId = request.ProductId;
        packSize.Name = request.Name;
        packSize.QtyInBaseUnit = request.QtyInBaseUnit;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeleteAsync(int id, CancellationToken ct)
    {
        var packSize = await repo.GetByIdAsync(id, ct);
        if (packSize is null) return ServiceError.NotFound;

        // No IsActive on PackSize (judgment call - see DECISIONS.md): small enough that a wrong
        // one is just deleted and recreated, so this is a real delete, not a soft deactivate.
        repo.Remove(packSize);
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    private static readonly System.Linq.Expressions.Expression<Func<PackSize, PackSizeDto>> ToDtoExpr =
        x => new PackSizeDto(x.PackSizeId, x.ProductId, x.Name, x.QtyInBaseUnit);

    private static PackSizeDto ToDto(PackSize x) => new(x.PackSizeId, x.ProductId, x.Name, x.QtyInBaseUnit);
}
