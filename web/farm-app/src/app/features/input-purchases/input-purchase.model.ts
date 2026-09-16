// Mirrors FarmApp.Api.Application.InputPurchases.InputPurchaseDtos. Unlike
// ProducePurchaseLineDto, a line here carries no StockBatchId - input stock has no batch/lot
// layer (see DECISIONS.md Phase 3a: InputStockMovement posts straight against InputItemId, no
// StockBatch in between), so there is nothing per-line to link back to beyond the movement ledger
// itself. On-hand after the purchase is read separately via InputItemsApiService.getOnHand().
export interface InputPurchaseLineDto {
  inputPurchaseLineId: number;
  inputItemId: number;
  qty: number;
  unitCost: number;
  vatAmount: number | null;
}

export interface InputPurchaseDto {
  inputPurchaseId: number;
  supplierId: number;
  date: string;
  invoiceRef: string | null;
  lines: InputPurchaseLineDto[];
}

// VatAmount is optional (doc 10 §2's VAT-readiness gap, same as ProducePurchase) - typing the VAT
// shown on the supplier's slip costs 5 seconds and makes history reclaimable/reportable later.
export interface CreateInputPurchaseLineRequest {
  inputItemId: number;
  qty: number;
  unitCost: number;
  vatAmount: number | null;
}

export interface CreateInputPurchaseRequest {
  supplierId: number;
  date: string;
  invoiceRef: string | null;
  lines: CreateInputPurchaseLineRequest[];
}
