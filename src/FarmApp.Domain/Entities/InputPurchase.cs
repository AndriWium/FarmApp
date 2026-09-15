namespace FarmApp.Domain.Entities;

/// <summary>Header for an input purchase from a supplier (fertiliser, seed, chemicals,
/// packaging) - one invoice, N lines. Mirrors ProducePurchase's shape (Phase 1b precedent): the
/// header carries no quantities or costs, those live on the lines.</summary>
public class InputPurchase
{
    public int InputPurchaseId { get; set; }
    public int SupplierId { get; set; } // plain FK column, no navigation (matches Block/Crop/Cultivar/ProducePurchase precedent)
    public DateTime Date { get; set; }
    public string? InvoiceRef { get; set; }
}
