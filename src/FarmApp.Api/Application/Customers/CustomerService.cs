using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Customers;

public class CustomerService(ICustomerRepository repo, IPriceListRepository priceListRepo, IUnitOfWork uow) : ICustomerService
{
    public Task<List<CustomerDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(ToDtoExpr, includeInactive, ct);

    public Task<CustomerDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDtoExpr, ct);

    public async Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerRequest request, CancellationToken ct)
    {
        if (await priceListRepo.GetByIdAsync(request.PriceListId, ct) is null)
            return ServiceResult<CustomerDto>.Fail(ServiceError.NotFound);

        // No duplicate-name check: customer names are not expected to be unique (matches Supplier).
        var customer = new Customer
        {
            Name = request.Name,
            Phone = request.Phone,
            Type = request.Type,
            PriceListId = request.PriceListId,
            CreditLimit = request.CreditLimit,
        };
        await repo.AddAsync(customer, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<CustomerDto>.Ok(ToDto(customer));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken ct)
    {
        var customer = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (customer is null) return ServiceError.NotFound;

        if (await priceListRepo.GetByIdAsync(request.PriceListId, ct) is null)
            return ServiceError.NotFound;

        customer.Name = request.Name;
        customer.Phone = request.Phone;
        customer.Type = request.Type;
        customer.PriceListId = request.PriceListId;
        customer.CreditLimit = request.CreditLimit;
        customer.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var customer = await repo.GetByIdAsync(id, ct);
        if (customer is null) return ServiceError.NotFound;

        customer.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    private static readonly System.Linq.Expressions.Expression<Func<Customer, CustomerDto>> ToDtoExpr =
        x => new CustomerDto(x.CustomerId, x.Name, x.Phone, x.Type, x.PriceListId, x.CreditLimit, x.IsActive);

    private static CustomerDto ToDto(Customer x) =>
        new(x.CustomerId, x.Name, x.Phone, x.Type, x.PriceListId, x.CreditLimit, x.IsActive);
}
