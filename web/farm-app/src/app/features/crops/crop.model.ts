// Mirrors FarmApp.Api.Application.Crops.CropDtos.
export interface CropDto {
  cropId: number;
  name: string;
  isActive: boolean;
}

export interface CreateCropRequest {
  name: string;
}

export interface UpdateCropRequest {
  name: string;
  isActive: boolean;
}
