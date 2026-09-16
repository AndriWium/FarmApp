// Mirrors FarmApp.Api.Application.PriceLists.PriceListDtos.
export interface PriceListDto {
  priceListId: number;
  name: string;
  isActive: boolean;
}

export interface CreatePriceListRequest {
  name: string;
}

export interface UpdatePriceListRequest {
  name: string;
  isActive: boolean;
}
