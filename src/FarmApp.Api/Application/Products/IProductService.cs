using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Products;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);

    // Recipe sub-resource (Prepared products) - modeled as part of Product, not a standalone
    // CRUD (see doc brief): the whole recipe is replaced atomically, never edited line by line.
    Task<ProductWithRecipeDto?> GetWithRecipeAsync(int productId, CancellationToken ct);
    Task<ServiceResult<ProductWithRecipeDto>> SetRecipeAsync(int productId, List<RecipeLineRequest> lines, CancellationToken ct);
}
