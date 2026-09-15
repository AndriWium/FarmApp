namespace FarmApp.Domain.Entities;

/// <summary>Header for a produce purchase from a supplier - one invoice, N lines. Each line
/// creates its own StockBatch + seeding PurchaseIn movement (see ProducePurchaseLine); the
/// header itself carries no quantities or costs, those live on the lines.</summary>
public class ProducePurchase
{
    public int ProducePurchaseId { get; set; }
    public int SupplierId { get; set; } // plain FK column, no navigation (matches Block/Crop/Cultivar precedent)
    public DateTime Date { get; set; }
    public string? InvoiceRef { get; set; }
}
