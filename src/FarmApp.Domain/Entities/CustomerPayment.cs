using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

/// <summary>A payment received against a customer's outstanding account balance - append-only,
/// never mutated or deleted, same "historical fact" spirit as StockMovement/Price (doc 02/11): a
/// wrong payment gets corrected by recording an adjusting entry, not editing history. A
/// customer's balance is never stored anywhere (doc 02's "derive, don't store" rule, same as
/// stock on-hand) - it's always SUM(SalePayment.Amount WHERE Method == Account, across that
/// customer's Complete sales) minus SUM(CustomerPayment.Amount for that customer)
/// (CustomerPaymentService.GetBalanceAsync).</summary>
public class CustomerPayment
{
    public int CustomerPaymentId { get; set; }
    public int CustomerId { get; set; } // plain FK column, no navigation (matches Sale/SalePayment precedent)
    public DateTime Date { get; set; }
    public decimal Amount { get; set; } // decimal(18,2)
    public CustomerPaymentMethod Method { get; set; }
    public string? Ref { get; set; } // reference number (EFT reference, receipt number, etc.)
}
