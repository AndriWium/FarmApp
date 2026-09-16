import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  CashFlowDto,
  DebtorAgingRowDto,
  HarvestSummaryRowDto,
  IncomeStatementDto,
  InputUsageRowDto,
  RainfallComparisonDto,
  SalesAnalysisRowDto,
  SeasonFarmingReportDto,
  StockMovementSummaryDto,
  StockOnHandDto,
  TillSessionOverShortDto,
} from './report.model';

// One service for every ReportsController endpoint (doc 04) rather than splitting by report
// group - eleven thin GETs against one `api/v1/reports` base, none of them share enough
// filter/paging shape to justify five separate injectable services (unlike, say,
// StockBatchesApiService vs StockMovementsApiService, which are genuinely different resources).
// Every screen behind these calls is already gated by ownerGuard (route) and CanViewReports
// (API) - a non-Owner never reaches a component that would call this service.
//
// Every from/to pair here is a plain yyyy-MM-dd date from a <input type="date">. The backend
// compares `SaleDateTime <= @to` etc. against a real DateTime, so a bare date string for `to`
// would parse as that day's midnight and silently exclude every sale/movement/session recorded
// later that same day - `endOfDay` pushes `to` to 23:59:59 so "to 30 Sep" genuinely means the
// whole of the 30th, not "up to the start of the 30th".
function endOfDay(date: string): string {
  return `${date}T23:59:59`;
}

@Injectable({ providedIn: 'root' })
export class ReportsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/reports`;

  getSalesAnalysis(from: string, to: string) {
    return this.http.get<SalesAnalysisRowDto[]>(`${this.url}/sales-analysis`, {
      params: { from, to: endOfDay(to) },
    });
  }

  getStockOnHand() {
    return this.http.get<StockOnHandDto[]>(`${this.url}/stock-on-hand`);
  }

  getStockMovementSummary(from: string, to: string, productId?: number | null, gradeId?: number | null) {
    const params: Record<string, string | number> = { from, to: endOfDay(to) };
    if (productId) params['productId'] = productId;
    if (gradeId) params['gradeId'] = gradeId;
    return this.http.get<StockMovementSummaryDto[]>(`${this.url}/stock-movement-summary`, { params });
  }

  getIncomeStatement(from: string, to: string) {
    return this.http.get<IncomeStatementDto>(`${this.url}/income-statement`, {
      params: { from, to: endOfDay(to) },
    });
  }

  getSeasonFarmingReport(seasonId: number) {
    return this.http.get<SeasonFarmingReportDto>(`${this.url}/season-farming/${seasonId}`);
  }

  getHarvestSummary(seasonId: number) {
    return this.http.get<HarvestSummaryRowDto[]>(`${this.url}/harvest-summary`, { params: { seasonId } });
  }

  getInputUsage(seasonId?: number | null, from?: string | null, to?: string | null) {
    const params: Record<string, string | number> = {};
    if (seasonId) params['seasonId'] = seasonId;
    if (from) params['from'] = from;
    if (to) params['to'] = endOfDay(to);
    return this.http.get<InputUsageRowDto[]>(`${this.url}/input-usage`, { params });
  }

  getRainfall(year: number, month: number) {
    return this.http.get<RainfallComparisonDto>(`${this.url}/rainfall`, { params: { year, month } });
  }

  getTillSessions(from: string, to: string) {
    return this.http.get<TillSessionOverShortDto[]>(`${this.url}/till-sessions`, {
      params: { from, to: endOfDay(to) },
    });
  }

  getCashFlow(from: string, to: string) {
    return this.http.get<CashFlowDto>(`${this.url}/cash-flow`, { params: { from, to: endOfDay(to) } });
  }

  getDebtorsAging() {
    return this.http.get<DebtorAgingRowDto[]>(`${this.url}/debtors-aging`);
  }
}
