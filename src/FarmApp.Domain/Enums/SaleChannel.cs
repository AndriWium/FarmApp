namespace FarmApp.Domain.Enums;

/// <summary>Where the sale happened - doc 01 §4's four sales channels for a farmer/fruit seller.
/// Informal covers "informal/bulk" (bakkie loads, negotiated prices) from the same list.</summary>
public enum SaleChannel
{
    FarmStall,
    Market,
    Wholesale,
    Informal,
}
