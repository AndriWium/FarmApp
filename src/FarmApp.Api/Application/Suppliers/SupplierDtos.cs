namespace FarmApp.Api.Application.Suppliers;

public record SupplierDto(int SupplierId, string Name, string? Phone, string? Notes, bool IsActive);

public record CreateSupplierRequest(string Name, string? Phone, string? Notes);

public record UpdateSupplierRequest(string Name, string? Phone, string? Notes, bool IsActive);
