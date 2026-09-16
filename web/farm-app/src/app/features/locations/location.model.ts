// Mirrors FarmApp.Api.Application.Locations.LocationDtos. Added in Phase 5c-1 - not part of the
// Phase 5b master-data sweep, but a Location record is a hard prerequisite for Transfer
// (FromLocationId/ToLocationId, required, per StockMovements) and useful as the optional
// LocationId on every other movement type, so a minimal CRUD screen (same shape as Grade - just
// a name + IsActive) is added now rather than leaving locations creatable only via Swagger.
export interface LocationDto {
  locationId: number;
  name: string;
  isActive: boolean;
}

export interface CreateLocationRequest {
  name: string;
}

export interface UpdateLocationRequest {
  name: string;
  isActive: boolean;
}
