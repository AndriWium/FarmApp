using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

/// <summary>The core POS transaction header - one TillSession sale, N SaleLines, N SalePayments,
/// all written together (Api/Application/Sales/SaleService.CreateSaleAsync, inside a real EF Core
/// transaction). ClientGuid is client-generated and unique (doc 08's idempotency mechanism):
/// CreateSaleAsync checks it first and returns the existing sale unchanged on replay rather than
/// reprocessing or re-depleting stock - this is what makes an offline-retry safe. Implements
/// IPeriodLocked, like StockMovement - anticipated by Phase 1a's DECISIONS.md entry ("Sale and
/// StockMovement (Phase 1/2) will be the first") - so a sale can't be posted into a closed
/// accounting month.</summary>
public class Sale : IPeriodLocked
{
    public int SaleId { get; set; }
    public int TillSessionId { get; set; } // plain FK column, no navigation
    public int? CustomerId { get; set; } // plain FK column, no navigation
    public DateTime DateTime { get; set; }
    public SaleChannel Channel { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Complete;
    public string? Notes { get; set; }
    public Guid ClientGuid { get; set; }

    DateTime IPeriodLocked.BusinessDate => DateTime;
}
