// Mirrors FarmApp.Api.Application.ActivityTypes.ActivityTypeDtos. Category is a plain string
// (not an enum) per doc 11/DECISIONS.md Phase 3a - the set of categories is meant to stay open,
// unlike a genuinely closed enum like InputItemCategory.
export interface ActivityTypeDto {
  activityTypeId: number;
  name: string;
  category: string;
  isActive: boolean;
}

export interface CreateActivityTypeRequest {
  name: string;
  category: string;
}

export interface UpdateActivityTypeRequest {
  name: string;
  category: string;
  isActive: boolean;
}
