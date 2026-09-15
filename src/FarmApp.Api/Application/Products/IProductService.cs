using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Products;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
