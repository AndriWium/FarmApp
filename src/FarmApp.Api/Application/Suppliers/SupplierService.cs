using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Suppliers;

public class SupplierService(ISupplierRepository repo, IUnitOfWork uow) : ISupplierService
{
    public Task<List<SupplierDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(s => new SupplierDto(s.SupplierId, s.Name, s.Phone, s.Notes, s.IsActive, s.VatNumber), includeInactive, ct);

    public Task<SupplierDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, s => new SupplierDto(s.SupplierId, s.Name, s.Phone, s.Notes, s.IsActive, s.VatNumber), ct);

    public async Task<ServiceResult<SupplierDto>> CreateAsync(CreateSupplierRequest request, CancellationToken ct)
    {
        // No duplicate-name check: supplier names are not expected to be unique.
        var supplier = new Supplier
        {
            Name = request.Name,
            Phone = request.Phone,
            Notes = request.Notes,
            VatNumber = request.VatNumber,
        };
        await repo.AddAsync(supplier, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<SupplierDto>.Ok(
            new SupplierDto(supplier.SupplierId, supplier.Name, supplier.Phone, supplier.Notes, supplier.IsActive, supplier.VatNumber));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken ct)
    {
        var supplier = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (supplier is null) return ServiceError.NotFound;

        supplier.Name = request.Name;
        supplier.Phone = request.Phone;
        supplier.Notes = request.Notes;
        supplier.IsActive = request.IsActive;
        supplier.VatNumber = request.VatNumber;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var supplier = await repo.GetByIdAsync(id, ct);
        if (supplier is null) return ServiceError.NotFound;

        supplier.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
