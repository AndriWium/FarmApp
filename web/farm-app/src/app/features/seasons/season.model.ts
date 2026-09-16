// Mirrors FarmApp.Api.Application.Seasons.SeasonDtos + Season's SeasonStatus enum. Program.cs
// serializes enums as strings, so Status travels as "Open"/"Closed" over the wire.
//
// EstimatedCostPerKg is always server-computed (SeasonService, via ISeasonCostCalculator) from
// ExpectedTotalCost/ExpectedYieldKg - it never appears on Create/UpdateSeasonRequest, only on the
// read-back SeasonDto. Status is likewise never client-writable - closing a season (the true-up
// flow, doc 09) is a separate screen, out of scope for this phase (task brief).
export type SeasonStatus = 'Open' | 'Closed';

export interface SeasonDto {
  seasonId: number;
  plantingId: number;
  name: string;
  startDate: string;
  endDate: string;
  status: SeasonStatus;
  expectedTotalCost: number | null;
  expectedYieldKg: number | null;
  estimatedCostPerKg: number | null;
}

export interface CreateSeasonRequest {
  plantingId: number;
  name: string;
  startDate: string;
  endDate: string;
  expectedTotalCost: number | null;
  expectedYieldKg: number | null;
}

// Rejected once Status is Closed (SeasonService.UpdateAsync -> ServiceError.SeasonAlreadyClosed) -
// the UI hides the Edit action on a closed season to match, see SeasonPageComponent.
export interface UpdateSeasonRequest {
  plantingId: number;
  name: string;
  startDate: string;
  endDate: string;
  expectedTotalCost: number | null;
  expectedYieldKg: number | null;
}
