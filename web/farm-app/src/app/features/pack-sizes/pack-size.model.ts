// Mirrors FarmApp.Api.Application.PackSizes.PackSizeDtos. Deliberately has NO isActive field
// (DECISIONS.md Phase 0b-2): a pack size is small enough that a mistake is just deleted and
// recreated, so the backend performs a real hard DELETE, not a soft-deactivate - the first (and
// so far only) hard-delete entity in the codebase. Uniqueness is scoped to (ProductId, Name), not
// global, so the same pack-size name (e.g. "5kg box") is fine under two different products.
export interface PackSizeDto {
  packSizeId: number;
  productId: number;
  name: string;
  qtyInBaseUnit: number;
}

export interface CreatePackSizeRequest {
  productId: number;
  name: string;
  qtyInBaseUnit: number;
}

export interface UpdatePackSizeRequest {
  productId: number;
  name: string;
  qtyInBaseUnit: number;
}
