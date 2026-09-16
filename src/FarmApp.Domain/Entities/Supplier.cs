namespace FarmApp.Domain.Entities;

public class Supplier
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Nullable - doc 10 §2's VAT-readiness gap (Phase 0 was supposed to add this;
    /// closed in Phase 4b). Needed on tax invoices later, if/when VAT registration happens.</summary>
    public string? VatNumber { get; set; }
}
