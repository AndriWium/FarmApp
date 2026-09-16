// Mirrors FarmApp.Api.Application.Blocks.BlockDtos.
export interface BlockDto {
  blockId: number;
  name: string;
  areaHectare: number;
  note: string;
  isActive: boolean;
}

export interface CreateBlockRequest {
  name: string;
  areaHectare: number;
  note: string;
}

export interface UpdateBlockRequest {
  name: string;
  areaHectare: number;
  note: string;
  isActive: boolean;
}
