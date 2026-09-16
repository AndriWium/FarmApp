// Mirrors FarmApp.Api.Application.Products.ProductDtos + the FarmApp.Domain.Enums used on it
// (ProductType, MakeMode, ProductBaseUnit). Program.cs registers JsonStringEnumConverter globally
// (see input-item.model.ts's comment), so these travel as strings, not numbers.
//
// CropId/MakeMode are nullable and NOT hard-restricted to their "natural" ProductType server-side
// (DECISIONS.md Phase 0b-2 - a real-world override is allowed), but the create/edit form only
// shows/enables CropId when ProductType is Produce, and MakeMode when Prepared, since showing a
// crop dropdown on a Resale product (for example) would just be confusing for a farmer filling
// this form in day to day, not a real workflow need.
export type ProductType = 'Produce' | 'Resale' | 'Prepared';
export type MakeMode = 'ToOrder' | 'Batch';
export type ProductBaseUnit = 'Kg' | 'Each';

export const PRODUCT_TYPES: ProductType[] = ['Produce', 'Resale', 'Prepared'];
export const MAKE_MODES: MakeMode[] = ['ToOrder', 'Batch'];
export const PRODUCT_BASE_UNITS: ProductBaseUnit[] = ['Kg', 'Each'];

export interface ProductDto {
  productId: number;
  name: string;
  productType: ProductType;
  cropId: number | null;
  makeMode: MakeMode | null;
  baseUnit: ProductBaseUnit;
  isActive: boolean;
}

export interface CreateProductRequest {
  name: string;
  productType: ProductType;
  cropId: number | null;
  makeMode: MakeMode | null;
  baseUnit: ProductBaseUnit;
}

export interface UpdateProductRequest {
  name: string;
  productType: ProductType;
  cropId: number | null;
  makeMode: MakeMode | null;
  baseUnit: ProductBaseUnit;
  isActive: boolean;
}
