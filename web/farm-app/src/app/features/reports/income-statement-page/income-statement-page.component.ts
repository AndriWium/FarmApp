import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { currentMonthRange, formatLocalDate, parseLocalDate } from '../../../shared/date-range.util';
import { IncomeStatementDto } from '../report.model';
import { ReportsApiService } from '../reports-api.service';

// Local-date arithmetic throughout (never toISOString()/new Date(str)) - see date-range.util.ts's
// file comment for why: this app runs in South Africa (UTC+2), where the UTC-based shortcuts
// silently shift the displayed date back a day.
function shiftYears(date: string, years: number): string {
  const d = parseLocalDate(date);
  return formatLocalDate(new Date(d.getFullYear() + years, d.getMonth(), d.getDate()));
}

/** Gross profit % of sales - undefined (not just 0) when there were no sales, so the template can
 *  show "-" instead of a misleading 0%. */
function gpPercent(statement: IncomeStatementDto): number | null {
  return statement.sales > 0 ? (statement.grossProfit / statement.sales) * 100 : null;
}

/**
 * Income statement (doc 04 §1), read as a statement rather than a raw table: Sales − COS −
 * Wastage = Gross profit (and GP%), − Expenses by category = Net profit. Shown for the selected
 * period alongside year-to-date and same-period-last-year columns (doc 04's "shown per month with
 * year-to-date and same-month-last-year columns", and the design notes' seasonality point -
 * month-vs-same-month-last-year matters more than month-vs-last-month for a farm). The API takes
 * one arbitrary [from, to] per call and has no multi-column endpoint, so this fires three parallel
 * GetIncomeStatement calls rather than adding new backend surface (task brief: read from
 * already-complete endpoints).
 */
@Component({
  selector: 'app-income-statement-page',
  imports: [DecimalPipe, FormsModule, RouterLink],
  templateUrl: './income-statement-page.component.html',
  styleUrl: './income-statement-page.component.scss',
})
export class IncomeStatementPageComponent implements OnInit {
  private api = inject(ReportsApiService);

  private defaultRange = currentMonthRange();
  from = signal(this.defaultRange.from);
  to = signal(this.defaultRange.to);

  loading = signal(true);
  error = signal('');

  period = signal<IncomeStatementDto | null>(null);
  yearToDate = signal<IncomeStatementDto | null>(null);
  lastYear = signal<IncomeStatementDto | null>(null);

  periodGpPercent = computed(() => {
    const s = this.period();
    return s ? gpPercent(s) : null;
  });
  lastYearGpPercent = computed(() => {
    const s = this.lastYear();
    return s ? gpPercent(s) : null;
  });

  // Drilldown query params for "see the transactions behind Sales/COS" - Sales Analysis is the
  // one report that carries per-sale-line detail for the same [from, to] window (doc 04's
  // "click a total -> see the transactions").
  salesAnalysisLink = computed(() => ({ from: this.from(), to: this.to() }));

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    const from = this.from();
    const to = this.to();

    const ytdFrom = `${to.slice(0, 4)}-01-01`;
    const lastYearFrom = shiftYears(from, -1);
    const lastYearTo = shiftYears(to, -1);

    forkJoin({
      period: this.api.getIncomeStatement(from, to),
      yearToDate: this.api.getIncomeStatement(ytdFrom, to),
      lastYear: this.api.getIncomeStatement(lastYearFrom, lastYearTo),
    }).subscribe({
      next: ({ period, yearToDate, lastYear }) => {
        this.period.set(period);
        this.yearToDate.set(yearToDate);
        this.lastYear.set(lastYear);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load the income statement. Please try again.');
        this.loading.set(false);
      },
    });
  }
}
