// Mirrors FarmApp.Api.Application.Activities.ActivityDtos.
//
// ActivityInputDto.UnitCost is read-back only - never client-supplied, always the weighted-average
// input cost snapshotted server-side at the moment of consumption (ActivityService.CreateActivityAsync).
export interface ActivityInputDto {
  activityInputId: number;
  inputItemId: number;
  qty: number;
  unitCost: number;
}

export interface ActivityDto {
  activityId: number;
  seasonId: number;
  activityTypeId: number;
  date: string;
  labourHours: number;
  labourCost: number;
  notes: string | null;
  inputs: ActivityInputDto[];
}

export interface CreateActivityInputLineRequest {
  inputItemId: number;
  qty: number;
}

// Inputs may be an empty array - not every activity consumes input stock (pruning, weeding, ...);
// CreateActivityRequestValidator only validates each line that exists, it doesn't require at
// least one (unlike Harvest's Lines, which must be non-empty).
export interface CreateActivityRequest {
  seasonId: number;
  activityTypeId: number;
  date: string;
  labourHours: number;
  labourCost: number;
  notes: string | null;
  inputs: CreateActivityInputLineRequest[];
}
