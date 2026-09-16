// Mirrors FarmApp.Api.Application.AccountingPeriods.AccountingPeriodDtos.

export interface AccountingPeriodDto {
  accountingPeriodId: number;
  year: number;
  month: number;
  status: 'Open' | 'Closed';
  closedAt: string | null;
  closedBy: string | null;
  reopenedAt: string | null;
  reopenReason: string | null;
}

export interface OpenTillSessionRowDto {
  tillSessionId: number;
  locationId: number;
  openedAt: string;
  openedBy: number;
}

export interface StockTakeVarianceRowDto {
  stockTakeId: number;
  date: string;
  stockTakeLineId: number;
  stockBatchId: number;
  variance: number;
}

export interface SeasonReviewRowDto {
  seasonId: number;
  name: string;
  status: string;
  estimatedCostPerKg: number | null;
  endDate: string;
}

// Mirrors FarmApp.Api.Application.Reports.WastageEntryRowDto (reused as-is by the checklist).
export interface WastageEntryRowDto {
  stockMovementId: number;
  date: string;
  productId: number;
  gradeId: number | null;
  qty: number;
  unitCost: number;
  value: number;
}

export interface CloseChecklistDto {
  year: number;
  month: number;
  tillSessionsAllClosed: boolean;
  openTillSessions: OpenTillSessionRowDto[];
  materialStockTakeVariances: StockTakeVarianceRowDto[];
  openSeasonsNeedingEstimate: SeasonReviewRowDto[];
  closedSeasonsMissingTrueUp: SeasonReviewRowDto[];
  wastageEntriesForReview: WastageEntryRowDto[];
  canClose: boolean;
}

export interface CloseMonthResultDto {
  period: AccountingPeriodDto;
  checklist: CloseChecklistDto;
}

export interface ReopenMonthRequest {
  reason: string;
}
