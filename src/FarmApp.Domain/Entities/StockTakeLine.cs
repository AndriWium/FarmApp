namespace FarmApp.Domain.Entities;

/// <summary>One counted batch within a StockTake. SystemQty is snapshotted from
/// IStockMovementRepository.GetOnHandAsync the instant the stock take starts
/// (StockTakeService.StartStockTakeAsync) - never trusted from the client, since the whole point
/// of a stock take is comparing a physical count against the server's own on-hand record at that
/// moment. CountedQty/Variance stay null/zero until StockTakeService.RecordCountsAsync fills
/// them in; Variance = CountedQty - SystemQty, signed (positive = found more than expected,
/// negative = found less).</summary>
public class StockTakeLine
{
    public int StockTakeLineId { get; set; }
    public int StockTakeId { get; set; } // plain FK column, no navigation
    public int StockBatchId { get; set; } // plain FK column, no navigation

    public decimal? CountedQty { get; set; } // decimal(18,3); null until counted
    public decimal SystemQty { get; set; } // decimal(18,3)
    public decimal Variance { get; set; } // decimal(18,3); 0 until counted
}
