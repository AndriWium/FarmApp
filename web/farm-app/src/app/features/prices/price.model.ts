// Mirrors FarmApp.Api.Application.Prices.PriceDtos. Only the read side (GetCurrent) is wrapped
// here - SetPrice is a CanManageMasterData (Owner) master-data action with no screen anywhere yet
// (not this phase's job, and not needed by the sell screen, which only ever reads a price).
export interface PriceDto {
  priceId: number;
  priceListId: number;
  productId: number;
  gradeId: number | null;
  packSizeId: number | null;
  unitPrice: number;
  validFrom: string;
  validTo: string | null;
}
