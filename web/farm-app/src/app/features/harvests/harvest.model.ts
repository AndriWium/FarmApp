// Mirrors FarmApp.Api.Application.Harvests.HarvestDtos.
//
// HarvestLineDto.UnitCost is read-back only (never client-supplied) - it's always
// Season.EstimatedCostPerKg snapshotted server-side at the moment of harvest (Phase 4a; see
// HarvestService.CreateHarvestAsync). StockBatchId is the batch this line created, for
// traceability, same precedent as ProducePurchaseLineDto.
export interface HarvestLineDto {
  harvestLineId: number;
  productId: number;
  gradeId: number | null;
  qtyKg: number;
  unitCost: number;
  stockBatchId: number;
}

// WithholdingOverrideReason is set only when the harvest was recorded inside a withholding-period
// lock on the season's block and a human explicitly overrode it - a food-safety-relevant field
// that must be easy to find on the detail view (doc 05 §5, task brief).
export interface HarvestDto {
  harvestId: number;
  seasonId: number;
  date: string;
  pickedBy: number | null;
  notes: string | null;
  withholdingOverrideReason: string | null;
  lines: HarvestLineDto[];
}

// ShelfLifeDays exists here purely to build the line's StockBatch (same shape as
// CreatePurchaseLineRequest.ShelfLifeDays) - it isn't persisted on HarvestLine itself. There is
// deliberately no UnitCost field - the backend always derives it from the season's current
// EstimatedCostPerKg (Phase 4a) and rejects the harvest outright (SeasonEstimateNotSet) if the
// season has no estimate set yet.
export interface CreateHarvestLineRequest {
  productId: number;
  gradeId: number | null;
  qtyKg: number;
  shelfLifeDays: number;
}

// WithholdingOverrideReason is required only when the harvest date falls inside a withholding
// lock on this season's block (doc 05 §5) - supplying it when the block isn't locked is harmless
// and simply ignored server-side.
export interface CreateHarvestRequest {
  seasonId: number;
  date: string;
  pickedBy: number | null;
  notes: string | null;
  withholdingOverrideReason: string | null;
  lines: CreateHarvestLineRequest[];
}
