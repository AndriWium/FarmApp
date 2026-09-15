using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

/// <summary>One payment against a Sale - a sale can split across multiple rows (e.g. part Card,
/// part Account) as long as they sum to the sale's total (SaleService.CreateSaleAsync enforces
/// this before writing anything). A customer's outstanding balance is never stored here or
/// anywhere else - it's always derived (doc 02's "derive, don't store" rule, same as stock
/// on-hand): Σ Sale amounts with an Account payment - Σ CustomerPayment amounts (CustomerPayment
/// doesn't exist until Phase 2b).</summary>
public class SalePayment
{
    public int SalePaymentId { get; set; }
    public int SaleId { get; set; } // plain FK column, no navigation
    public SalePaymentMethod Method { get; set; }
    public decimal Amount { get; set; } // decimal(18,2)
}
