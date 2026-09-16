// Mirrors FarmApp.Api.Application.StockMovements.StockMovementDtos +
// FarmApp.Domain.Enums.StockMovementType.
export type StockMovementType =
  | 'HarvestIn'
  | 'PurchaseIn'
  | 'SaleOut'
  | 'Wastage'
  | 'OwnUse'
  | 'Sample'
  | 'Donation'
  | 'Repack'
  | 'Adjustment'
  | 'TransferOut'
  | 'TransferIn';

export interface StockMovementDto {
  stockMovementId: number;
  stockBatchId: number;
  date: string;
  type: StockMovementType;
  qty: number;
  refTable: string | null;
  refId: number | null;
  reason: string | null;
  locationId: number | null;
}

export interface RecordStockMovementRequest {
  productId: number;
  gradeId: number | null;
  qty: number;
  reason: string | null;
  locationId: number | null;
}

export interface TransferStockRequest {
  productId: number;
  gradeId: number | null;
  qty: number;
  fromLocationId: number;
  toLocationId: number;
  reason: string | null;
}

export interface StockOnHandSummaryDto {
  productId: number;
  productName: string;
  gradeId: number | null;
  gradeName: string | null;
  qtyOnHand: number;
  value: number;
}

// The 7 depleting/moving actions this phase's UI offers, one route segment each - matches
// POST /api/v1/stock-movements/{kind} exactly (Phase 1a/1b's controller, verified in
// StockMovementsController.cs). "simple" kinds all share RecordStockMovementRequest's shape
// (ProductId, GradeId, Qty, Reason, LocationId); Transfer alone uses TransferStockRequest
// (FromLocationId/ToLocationId instead of one LocationId).
export type SimpleMovementKind =
  | 'wastage'
  | 'own-use'
  | 'sample'
  | 'donation'
  | 'adjustment'
  | 'repack';
export type MovementKind = SimpleMovementKind | 'transfer';

export interface MovementKindMeta {
  kind: MovementKind;
  label: string;
  description: string;
}

export const MOVEMENT_KINDS: MovementKindMeta[] = [
  {
    kind: 'wastage',
    label: 'Wastage',
    description: 'Stock spoiled, damaged, or otherwise lost - not sold, not used.',
  },
  {
    kind: 'own-use',
    label: 'Own Use',
    description: "Stock taken for the household or farm's own consumption, not sold.",
  },
  {
    kind: 'sample',
    label: 'Sample',
    description: 'Stock given away as a taste/trial sample to a customer.',
  },
  {
    kind: 'donation',
    label: 'Donation',
    description: 'Stock donated (e.g. to a charity or food bank).',
  },
  {
    kind: 'adjustment',
    label: 'Adjustment',
    description: 'A downward correction to on-hand quantity, e.g. after a stock-take shortfall.',
  },
  {
    kind: 'repack',
    label: 'Repack',
    description: 'Stock leaving its current tracked form/pack size (e.g. crate broken into punnets).',
  },
  {
    kind: 'transfer',
    label: 'Transfer',
    description: 'Stock moved from one location to another (e.g. farm store to market stall).',
  },
];
