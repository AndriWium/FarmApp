namespace FarmApp.Domain.Enums;

/// <summary>Deliberately smaller than StockMovementType (doc 02's produce ledger has 11 values
/// for FIFO/batch/sale/transfer nuance) - InputItem has no grade/batch/FIFO complexity (it's not
/// sold, just consumed), so three values are enough (Phase 3a task brief).</summary>
public enum InputStockMovementType
{
    PurchaseIn,
    Consumption,
    Adjustment,
}
