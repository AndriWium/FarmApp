// Mirrors FarmApp.Api.Application.Reports.ReportDtos exactly - see that file's own doc comments
// for the reasoning behind each shape (revenue-attribution approximation, IsEstimate on an open
// season, etc.). This is a straight field-for-field TS mirror, same convention as every other
// feature's *.model.ts.

export interface SalesAnalysisRowDto {
  saleLineId: number;
  saleId: number;
  saleDateTime: string;
  channel: string;
  customerId: number | null;
  productId: number;
  productName: string;
  gradeId: number | null;
  gradeName: string | null;
  packSizeId: number | null;
  packSizeName: string | null;
  qty: number;
  unitPrice: number;
  discountAmount: number;
  discountReason: string | null;
  costAtSale: number;
  lineTotal: number;
  grossMargin: number;
}

export interface StockOnHandDto {
  productId: number;
  productName: string;
  gradeId: number | null;
  gradeName: string | null;
  qtyOnHand: number;
  value: number;
}

export interface StockMovementSummaryDto {
  productId: number;
  productName: string;
  gradeId: number | null;
  gradeName: string | null;
  openingBalance: number;
  totalIn: number;
  totalOut: number;
  closingBalance: number;
}

export interface ExpenseCategoryTotalDto {
  expenseCategoryId: number;
  categoryName: string;
  total: number;
}

export interface IncomeStatementDto {
  sales: number;
  cos: number;
  grossProfit: number;
  wastage: number;
  expensesByCategory: ExpenseCategoryTotalDto[];
  totalExpenses: number;
  netProfit: number;
}

// ---- Farming report (doc 04 §4) ----

export interface SeasonFarmingReportDto {
  seasonId: number;
  seasonName: string;
  status: string;
  inputCost: number;
  labourCost: number;
  overheadAllocated: number;
  totalCost: number;
  kgHarvested: number;
  costPerKg: number;
  revenueAttributed: number;
  margin: number;
  isEstimate: boolean;
}

export interface HarvestSummaryRowDto {
  productId: number;
  productName: string;
  gradeId: number | null;
  gradeName: string | null;
  currentSeasonKg: number;
  priorSeasonKg: number | null;
}

export interface InputUsageRowDto {
  inputItemId: number;
  inputItemName: string;
  qtyUsed: number;
  cost: number;
}

export interface RainfallComparisonDto {
  year: number;
  month: number;
  thisPeriodMm: number;
  historicalAverageMm: number | null;
  yearsCompared: number;
}

// ---- Cash & debtors report (doc 04 §5) ----

export interface TillSessionOverShortDto {
  tillSessionId: number;
  locationId: number;
  openedAt: string;
  openedBy: number;
  closedAt: string;
  systemCardTotal: number | null;
  cardMachineBatchTotal: number | null;
  difference: number | null;
  differenceNote: string | null;
}

export interface CashFlowDto {
  cashInCardEft: number;
  cashInDebtorPayments: number;
  cashIn: number;
  cashOutExpenses: number;
  cashOutPurchases: number;
  cashOut: number;
  net: number;
}

export interface DebtorAgingRowDto {
  customerId: number;
  customerName: string;
  current: number;
  days30: number;
  days60: number;
  days90Plus: number;
  total: number;
}
