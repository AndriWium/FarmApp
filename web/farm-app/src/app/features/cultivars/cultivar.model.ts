// Mirrors FarmApp.Api.Application.Cultivars.CultivarDtos. CropId is a required FK (doc 11 Angular
// conventions: models mirror API DTOs exactly) - the create/edit form resolves it via a <select>
// of Crops fetched through CropsApiService (Cultivar's screen depends on Crop's, per the Phase 5b-1
// task brief).
export interface CultivarDto {
  cultivarId: number;
  cropId: number;
  name: string;
  isActive: boolean;
}

export interface CreateCultivarRequest {
  cropId: number;
  name: string;
}

export interface UpdateCultivarRequest {
  cropId: number;
  name: string;
  isActive: boolean;
}
