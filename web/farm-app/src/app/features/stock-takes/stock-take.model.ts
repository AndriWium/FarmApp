// Mirrors FarmApp.Api.Application.StockTakes.StockTakeDtos. SystemQty is snapshotted
// server-side the moment the stock take starts (GetOnHandAsync at that instant) - the client
// never sends it and never recomputes it. CountedQty is null until RecordCountsAsync is called
// for that line; Variance is 0 until then too (both server-owned).
export interface StockTakeLineDto {
  stockTakeLineId: number;
  stockBatchId: number;
  countedQty: number | null;
  systemQty: number;
  variance: number;
}

export interface StockTakeDto {
  stockTakeId: number;
  date: string;
  locationId: number | null;
  notes: string | null;
  lines: StockTakeLineDto[];
}

// Starts a stock take against a known set of existing batches - a stock take never creates
// batches, only counts ones that already exist.
export interface StartStockTakeRequest {
  date: string;
  locationId: number | null;
  notes: string | null;
  stockBatchIds: number[];
}

// One physical count against a line created by starting the stock take.
export interface CountLineRequest {
  stockTakeLineId: number;
  countedQty: number;
}

export interface RecordCountsRequest {
  counts: CountLineRequest[];
}
