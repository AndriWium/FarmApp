// Mirrors FarmApp.Api.Application.Plantings.PlantingDtos + FarmApp.Domain.Enums.PlantingType.
// Program.cs serializes enums as strings (JsonStringEnumConverter), so Type travels as
// "Annual"/"Perennial" over the wire, not as a number.
//
// No IsActive/deactivate here (DECISIONS.md Phase 3a: neither Planting nor Season gets a soft-
// delete flag or a delete endpoint - a Planting's EndDate already distinguishes "still growing"
// from "finished").
export type PlantingType = 'Annual' | 'Perennial';

export const PLANTING_TYPES: PlantingType[] = ['Annual', 'Perennial'];

export interface PlantingDto {
  plantingId: number;
  blockId: number;
  cultivarId: number;
  startDate: string;
  endDate: string | null;
  type: PlantingType;
  plantCount: number | null;
  notes: string | null;
}

export interface CreatePlantingRequest {
  blockId: number;
  cultivarId: number;
  startDate: string;
  endDate: string | null;
  type: PlantingType;
  plantCount: number | null;
  notes: string | null;
}

export interface UpdatePlantingRequest {
  blockId: number;
  cultivarId: number;
  startDate: string;
  endDate: string | null;
  type: PlantingType;
  plantCount: number | null;
  notes: string | null;
}
