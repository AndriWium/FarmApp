using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Suppliers;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<SupplierDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<SupplierDto>> CreateAsync(CreateSupplierRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
