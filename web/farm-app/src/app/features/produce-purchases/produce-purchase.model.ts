// Mirrors FarmApp.Api.Application.ProducePurchases.ProducePurchaseDtos. StockBatchId on the line
// DTO is the batch created from this line (doc 02: ProducePurchaseLine "creates StockBatch") -
// it's not a persisted column, just traceability the backend looks up and includes.
export interface ProducePurchaseLineDto {
  producePurchaseLineId: number;
  productId: number;
  gradeId: number | null;
  qty: number;
  unitCost: number;
  vatAmount: number | null;
  stockBatchId: number;
}

export interface ProducePurchaseDto {
  producePurchaseId: number;
  supplierId: number;
  date: string;
  invoiceRef: string | null;
  lines: ProducePurchaseLineDto[];
}

// ShelfLifeDays isn't part of the readback line DTO (it's not persisted on the line itself) but
// IS required on a create-line request - the backend needs it to build the line's StockBatch
// (see ProducePurchaseDtos.cs's comment / DECISIONS.md). VatAmount is optional (doc 10 §2's
// VAT-readiness gap).
export interface CreatePurchaseLineRequest {
  productId: number;
  gradeId: number | null;
  qty: number;
  unitCost: number;
  shelfLifeDays: number;
  vatAmount: number | null;
}

export interface CreatePurchaseRequest {
  supplierId: number;
  date: string;
  invoiceRef: string | null;
  lines: CreatePurchaseLineRequest[];
}
