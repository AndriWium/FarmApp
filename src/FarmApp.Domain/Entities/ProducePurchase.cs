namespace FarmApp.Domain.Entities;

/// <summary>Header for a produce purchase from a supplier - one invoice, N lines. Each line
/// creates its own StockBatch + seeding PurchaseIn movement (see ProducePurchaseLine); the
/// header itself carries no quantities or costs, those live on the lines. Implements
/// IPeriodLocked (BusinessDate => Date, reusing the existing Date column, no new column needed)
/// so a purchase dated into a Closed accounting period is rejected at save time, same as every
/// other transactional entity - see DECISIONS.md.</summary>
public class ProducePurchase : IPeriodLocked
{
    public int ProducePurchaseId { get; set; }
    public int SupplierId { get; set; } // plain FK column, no navigation (matches Block/Crop/Cultivar precedent)
    public DateTime Date { get; set; }
    public string? InvoiceRef { get; set; }

    DateTime IPeriodLocked.BusinessDate => Date;
}
