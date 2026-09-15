using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Products;

public class ProductService(IProductRepository repo, ICropRepository cropRepo, IUnitOfWork uow) : IProductService
{
    public Task<List<ProductDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(ToDtoExpr, includeInactive, ct);

    public Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDtoExpr, ct);

    public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        // CropId only meaningful for Produce, MakeMode only for Prepared, but not hard-enforced
        // (doc brief - a human should be able to override real-world nuance).
        if (request.CropId is not null && await cropRepo.GetByIdAsync(request.CropId.Value, ct) is null)
            return ServiceResult<ProductDto>.Fail(ServiceError.NotFound);

        if (await repo.ExistsByNameAsync(request.Name, excludeId: null, ct))
            return ServiceResult<ProductDto>.Fail(ServiceError.DuplicateName);

        var product = new Product
        {
            Name = request.Name,
            ProductType = request.ProductType,
            CropId = request.CropId,
            MakeMode = request.MakeMode,
            BaseUnit = request.BaseUnit,
        };
        await repo.AddAsync(product, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<ProductDto>.Ok(ToDto(product));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (product is null) return ServiceError.NotFound;

        if (request.CropId is not null && await cropRepo.GetByIdAsync(request.CropId.Value, ct) is null)
            return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        product.Name = request.Name;
        product.ProductType = request.ProductType;
        product.CropId = request.CropId;
        product.MakeMode = request.MakeMode;
        product.BaseUnit = request.BaseUnit;
        product.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(id, ct);
        if (product is null) return ServiceError.NotFound;

        product.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    private static readonly System.Linq.Expressions.Expression<Func<Product, ProductDto>> ToDtoExpr =
        x => new ProductDto(x.ProductId, x.Name, x.ProductType, x.CropId, x.MakeMode, x.BaseUnit, x.IsActive);

    private static ProductDto ToDto(Product x) =>
        new(x.ProductId, x.Name, x.ProductType, x.CropId, x.MakeMode, x.BaseUnit, x.IsActive);
}
