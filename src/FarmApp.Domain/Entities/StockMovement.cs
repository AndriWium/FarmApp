using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

/// <summary>Append-only ledger row - never updated or deleted, only inserted. On-hand per batch
/// is always SUM(Qty) over these rows (doc 02); StockBatch itself carries no mutable quantity.
/// Implements IPeriodLocked (the interceptor was scaffolded in Phase 0a anticipating this
/// entity - see DECISIONS.md) so a movement can't be posted into a closed accounting month.</summary>
public class StockMovement : IPeriodLocked
{
    public int StockMovementId { get; set; }
    public int StockBatchId { get; set; } // plain FK column, no navigation
    public DateTime Date { get; set; }
    public StockMovementType Type { get; set; }

    /// <summary>Signed: positive for stock coming in (HarvestIn/PurchaseIn/TransferIn), negative
    /// for stock going out (Wastage/OwnUse/Sample/Donation/TransferOut/SaleOut). The sign is a
    /// business rule the service layer enforces when it constructs a movement - callers never
    /// pass a signed value in directly.</summary>
    public decimal Qty { get; set; } // decimal(18,3)

    /// <summary>Loose polymorphic pointer back to whatever header record caused this movement
    /// (e.g. "ProducePurchaseLine"/123), for traceability. Null for this phase's own
    /// non-header movement types (wastage, own-use, etc. have no header row to point at).</summary>
    public string? RefTable { get; set; }
    public int? RefId { get; set; }

    public string? Reason { get; set; }
    public int? LocationId { get; set; } // plain FK column, no navigation

    DateTime IPeriodLocked.BusinessDate => Date;
}
