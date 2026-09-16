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

// Mirrors FarmApp.Api.Application.WithholdingLocks.WithholdingStatusDto - the proactive
// warn-before-you-submit check (doc 05 §5) behind GET /blocks/{id}/withholding-status?date=.
// LockedUntil/Reason are null when not locked. Reason (when set) already names the block, the
// locked-until date, and the exact chemical/spray Activity responsible - built server-side, safe
// to show verbatim.
export interface WithholdingStatusDto {
  isLocked: boolean;
  lockedUntil: string | null;
  reason: string | null;
}
