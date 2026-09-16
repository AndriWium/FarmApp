namespace FarmApp.Api.Application.Suppliers;

public record SupplierDto(int SupplierId, string Name, string? Phone, string? Notes, bool IsActive, string? VatNumber);

/// <summary>VatNumber is optional (doc 10 §2's VAT-readiness gap) - needed on tax invoices later
/// if/when VAT registration happens.</summary>
public record CreateSupplierRequest(string Name, string? Phone, string? Notes, string? VatNumber = null);

public record UpdateSupplierRequest(string Name, string? Phone, string? Notes, bool IsActive, string? VatNumber = null);
