namespace FarmApp.Domain.Enums;

/// <summary>Card-only decision plus EFT and on-account (doc 01 §4: "no cash handling"); the enum
/// stays extensible if cash ever returns (doc 02's own callout on SalePayment).</summary>
public enum SalePaymentMethod
{
    Card,
    EFT,
    Account,
}
