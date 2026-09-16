// Mirrors FarmApp.Api.Application.RoadmapItems.RoadmapItemDtos.

export type RoadmapItemStatus = 'Planned' | 'InProgress' | 'Done';

export interface RoadmapItemDto {
  roadmapItemId: number;
  title: string;
  description: string;
  status: RoadmapItemStatus;
  sortOrder: number;
  targetPhase: string | null;
  completedOn: string | null;
}

export interface CreateRoadmapItemRequest {
  title: string;
  description: string;
  status: RoadmapItemStatus;
  sortOrder: number;
  targetPhase: string | null;
}

// CompletedOn is deliberately absent - the backend derives it from the Status transition, it is
// never client-supplied (see RoadmapItemService.UpdateAsync).
export interface UpdateRoadmapItemRequest {
  title: string;
  description: string;
  status: RoadmapItemStatus;
  sortOrder: number;
  targetPhase: string | null;
}
