namespace FarmApp.Api.Application.StockTakes;

public record StockTakeLineDto(
    int StockTakeLineId, int StockBatchId, decimal? CountedQty, decimal SystemQty, decimal Variance);

public record StockTakeDto(
    int StockTakeId, DateTime Date, int? LocationId, string? Notes, List<StockTakeLineDto> Lines);

/// <summary>Starts a stock take against a known set of existing batches. SystemQty is never
/// taken from the client - the service snapshots it itself via GetOnHandAsync the moment the
/// stock take starts (task brief).</summary>
public record StartStockTakeRequest(DateTime Date, int? LocationId, string? Notes, List<int> StockBatchIds);

/// <summary>One physical count against a line created by StartStockTakeAsync.</summary>
public record CountLineRequest(int StockTakeLineId, decimal CountedQty);

public record RecordCountsRequest(List<CountLineRequest> Counts);
