// Mirrors FarmApp.Api.Application.StockBatches.StockBatchDtos + FarmApp.Domain.Enums.StockSource.
// BestBeforeDate is computed server-side (Date + ShelfLifeDays) - never derived client-side, so
// the UI trusts whatever the API returns. On-hand is NOT a field on this DTO (doc 02: on-hand is
// always derived from the movement ledger, never stored) - the stock-on-hand screen fetches it
// per batch via GET /stock-batches/{id}/on-hand.
export type StockSource = 'Harvest' | 'Purchase' | 'Production';

export interface StockBatchDto {
  stockBatchId: number;
  productId: number;
  gradeId: number | null;
  source: StockSource;
  harvestId: number | null;
  purchaseLineId: number | null;
  productionBatchId: number | null;
  date: string;
  qtyIn: number;
  unitCost: number;
  shelfLifeDays: number;
  bestBeforeDate: string;
}
