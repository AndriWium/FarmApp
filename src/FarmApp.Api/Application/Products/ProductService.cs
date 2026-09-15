using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Products;

public class ProductService(
    IProductRepository repo,
    ICropRepository cropRepo,
    IRecipeLineRepository recipeLineRepo,
    IInputItemRepository inputItemRepo,
    IUnitOfWork uow) : IProductService
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

    public async Task<ProductWithRecipeDto?> GetWithRecipeAsync(int productId, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(productId, ct);
        if (product is null) return null;

        var lines = await recipeLineRepo.GetByProductIdAsync(
            productId, x => new RecipeLineDto(x.RecipeLineId, x.InputItemId, x.Qty), ct);
        return ToWithRecipeDto(product, lines);
    }

    public async Task<ServiceResult<ProductWithRecipeDto>> SetRecipeAsync(
        int productId, List<RecipeLineRequest> lines, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(productId, ct);
        if (product is null) return ServiceResult<ProductWithRecipeDto>.Fail(ServiceError.NotFound);

        // Validate every ingredient exists before mutating anything - reject the whole call
        // if any InputItemId is bad, don't partially apply a broken recipe.
        foreach (var line in lines)
        {
            if (await inputItemRepo.GetByIdAsync(line.InputItemId, ct) is null)
                return ServiceResult<ProductWithRecipeDto>.Fail(ServiceError.NotFound);
        }

        var existing = await recipeLineRepo.GetByProductIdAsync(productId, ct);
        recipeLineRepo.RemoveRange(existing);

        var newLines = lines
            .Select(l => new RecipeLine { ProductId = productId, InputItemId = l.InputItemId, Qty = l.Qty })
            .ToList();
        await recipeLineRepo.AddRangeAsync(newLines, ct);

        await uow.SaveChangesAsync(ct);   // one SaveChanges for the whole replace - atomic

        var dtoLines = newLines.Select(x => new RecipeLineDto(x.RecipeLineId, x.InputItemId, x.Qty)).ToList();
        return ServiceResult<ProductWithRecipeDto>.Ok(ToWithRecipeDto(product, dtoLines));
    }

    private static ProductWithRecipeDto ToWithRecipeDto(Product x, List<RecipeLineDto> lines) =>
        new(x.ProductId, x.Name, x.ProductType, x.CropId, x.MakeMode, x.BaseUnit, x.IsActive, lines);

    private static readonly System.Linq.Expressions.Expression<Func<Product, ProductDto>> ToDtoExpr =
        x => new ProductDto(x.ProductId, x.Name, x.ProductType, x.CropId, x.MakeMode, x.BaseUnit, x.IsActive);

    private static ProductDto ToDto(Product x) =>
        new(x.ProductId, x.Name, x.ProductType, x.CropId, x.MakeMode, x.BaseUnit, x.IsActive);
}
