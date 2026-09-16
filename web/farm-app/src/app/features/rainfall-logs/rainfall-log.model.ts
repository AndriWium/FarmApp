// Mirrors FarmApp.Api.Application.RainfallLogs.RainfallLogDtos. RainfallLogId is a surrogate int
// key (DECISIONS.md Phase 3a: Date is enforced unique via a DB index, not used as the primary
// key, matching every other entity's int-id shape). UpdateRainfallLogRequest deliberately excludes
// Date - only Mm/Notes can be corrected after entry; moving which day a reading belongs to isn't
// supported (no delete endpoint either).
export interface RainfallLogDto {
  rainfallLogId: number;
  date: string;
  mm: number;
  notes: string | null;
}

export interface CreateRainfallLogRequest {
  date: string;
  mm: number;
  notes: string | null;
}

export interface UpdateRainfallLogRequest {
  mm: number;
  notes: string | null;
}
