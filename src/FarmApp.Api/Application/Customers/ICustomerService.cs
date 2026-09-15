using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Customers;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<CustomerDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
