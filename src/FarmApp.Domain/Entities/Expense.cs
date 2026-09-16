namespace FarmApp.Domain.Entities;

/// <summary>A recorded farm expense (fuel, repairs, wages, a supplier slip) - append-only,
/// same "historical fact" spirit as StockMovement/CustomerPayment (doc 02/11): a wrong entry
/// gets corrected by recording an adjusting/replacement entry, not editing history. Not master
/// data in the Grade sense - no IsActive/soft-delete, plain create/read (Phase 4b task brief).
/// Implements IPeriodLocked (BusinessDate => Date) - same reasoning as Activity/Sale/
/// StockMovement (Phase 3a DECISIONS.md): a dated, transactional entity of the same shape, and
/// doc 10 §1's rule is stated generally, not scoped to specific entities.</summary>
public class Expense : IPeriodLocked
{
    public int ExpenseId { get; set; }
    public DateTime Date { get; set; }
    public int ExpenseCategoryId { get; set; } // plain FK column, no navigation (matches codebase convention)
    public decimal Amount { get; set; } // decimal(18,2)

    /// <summary>VAT shown on the slip, nullable - doc 10 §2's VAT-readiness gap. Not used by any
    /// calculation yet (the business isn't VAT-registered).</summary>
    public decimal? VatAmount { get; set; } // decimal(18,2)

    public int? SupplierId { get; set; } // plain FK column, no navigation; optional - not every expense has a supplier (e.g. wages)
    public int? SeasonId { get; set; } // plain FK column, no navigation; optional block/season allocation (doc 02)
    public string? Notes { get; set; }

    /// <summary>Path to a stored file (a photo of the slip, doc 02) - just a string this phase;
    /// no file-upload endpoint exists yet (task brief explicitly out of scope), so this is
    /// caller-supplied and unvalidated beyond a length cap.</summary>
    public string? AttachmentPath { get; set; }

    DateTime IPeriodLocked.BusinessDate => Date;
}
