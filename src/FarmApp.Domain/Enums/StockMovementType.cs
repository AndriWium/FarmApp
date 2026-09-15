namespace FarmApp.Domain.Enums;

/// <summary>The full set per doc 02, including SaleOut which no write path uses until Phase 2's
/// POS triggers it — the schema/enum is future-proofed now so no later migration is needed.</summary>
public enum StockMovementType
{
    HarvestIn,
    PurchaseIn,
    SaleOut,
    Wastage,
    OwnUse,
    Sample,
    Donation,
    Repack,
    Adjustment,
    TransferOut,
    TransferIn,
}
