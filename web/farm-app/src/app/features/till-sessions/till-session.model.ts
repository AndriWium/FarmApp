// Mirrors FarmApp.Api.Application.TillSessions.TillSessionDtos. A till session gates all POS
// selling (doc 01 Module 4) - Sale.TillSessionId is required and SaleService rejects a sale
// against a closed one (ServiceError.TillSessionClosed).
export interface TillSessionDto {
  tillSessionId: number;
  locationId: number;
  openedAt: string;
  openedBy: number;
  closedAt: string | null;
  systemCardTotal: number | null;
  cardMachineBatchTotal: number | null;
  difference: number | null;
  differenceNote: string | null;
}

export interface OpenTillSessionRequest {
  locationId: number;
}

// Day close (Phase 5d-2) - included now for a complete mirror of TillSessionsController, not used
// by this phase's sell screen.
export interface CloseTillSessionRequest {
  cardMachineBatchTotal: number;
  differenceNote: string | null;
}
