// Mirrors FarmApp.Api.Application.Sales.SaleDtos + FarmApp.Domain.Enums (SaleChannel,
// SalePaymentMethod, SaleStatus). Program.cs's global JsonStringEnumConverter means these travel
// as strings (see product.model.ts's comment) - the literal unions below must match the C# enum
// member names exactly.
export type SaleChannel = 'FarmStall' | 'Market' | 'Wholesale' | 'Informal';
export const SALE_CHANNELS: SaleChannel[] = ['FarmStall', 'Market', 'Wholesale', 'Informal'];

export type SalePaymentMethod = 'Card' | 'EFT' | 'Account';
export const SALE_PAYMENT_METHODS: SalePaymentMethod[] = ['Card', 'EFT', 'Account'];

export type SaleStatus = 'Complete' | 'Refunded';

export interface CreateSaleLineRequest {
  productId: number;
  gradeId: number | null;
  packSizeId: number | null;
  qty: number;
  unitPrice: number;
  discountAmount: number;
  discountReason: string | null;
}

export interface CreateSalePaymentRequest {
  method: SalePaymentMethod;
  amount: number;
}

// ClientGuid is client-generated (doc 08) - see pos-page.component.ts for the "generate once per
// basket, only rotate after a genuine success" rule that makes a flaky-connection retry safe.
export interface CreateSaleRequest {
  clientGuid: string;
  tillSessionId: number;
  customerId: number | null;
  channel: SaleChannel;
  notes: string | null;
  lines: CreateSaleLineRequest[];
  payments: CreateSalePaymentRequest[];
}

export interface SaleLineDto {
  saleLineId: number;
  productId: number;
  gradeId: number | null;
  packSizeId: number | null;
  qty: number;
  unitPrice: number;
  discountAmount: number;
  discountReason: string | null;
  costAtSale: number;
}

export interface SalePaymentDto {
  salePaymentId: number;
  method: SalePaymentMethod;
  amount: number;
}

export interface SaleDto {
  saleId: number;
  tillSessionId: number;
  customerId: number | null;
  dateTime: string;
  channel: SaleChannel;
  status: SaleStatus;
  notes: string | null;
  clientGuid: string;
  lines: SaleLineDto[];
  payments: SalePaymentDto[];
}

// Refund/day-close (Phase 5d-2) - included for a complete mirror of SalesController, not used by
// this phase's sell screen.
export interface RefundSaleRequest {
  reason: string | null;
}
