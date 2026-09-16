import { DatePipe, DecimalPipe, TitleCasePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { CustomerDto } from '../../customers/customer.model';
import { CustomersApiService } from '../../customers/customers-api.service';
import { currentMonthRange, precedingPeriod } from '../../../shared/date-range.util';
import { SalesAnalysisRowDto } from '../report.model';
import { ReportsApiService } from '../reports-api.service';

export type GroupBy = 'product' | 'grade' | 'channel' | 'customer' | 'dayOfWeek';

interface GroupRow {
  key: string;
  label: string;
  qty: number;
  revenue: number;
  discount: number;
  cost: number;
  margin: number;
  avgPricePerKg: number | null;
  prevAvgPricePerKg: number | null; // only populated when groupBy === 'product'
}

interface DiscountReasonRow {
  reason: string;
  amount: number;
  count: number;
}

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

/**
 * Sales analysis (doc 04 §2): sales by product/grade/pack size/channel/customer/day-of-week for a
 * date range, sortable/filterable, with discount totals by reason and an avg-price-per-kg-vs-last-
 * period spot-price-drift comparison (product grouping only - "vs last month" only means something
 * per product). Detail rows stay drillable straight through to the actual Sale (doc 04's "click a
 * total -> see the transactions") via routerLink to /sales/:id, SalesController's own detail route.
 *
 * Reads optional from/to query params so IncomeStatementPageComponent's "see the transactions
 * behind this" link can deep-link into the same period.
 */
@Component({
  selector: 'app-sales-analysis-page',
  imports: [DatePipe, DecimalPipe, TitleCasePipe, FormsModule, RouterLink],
  templateUrl: './sales-analysis-page.component.html',
  styleUrl: './sales-analysis-page.component.scss',
})
export class SalesAnalysisPageComponent implements OnInit {
  private api = inject(ReportsApiService);
  private customersApi = inject(CustomersApiService);
  private route = inject(ActivatedRoute);

  private defaultRange = currentMonthRange();
  from = signal(this.defaultRange.from);
  to = signal(this.defaultRange.to);
  groupBy = signal<GroupBy>('product');

  loading = signal(true);
  error = signal('');

  rows = signal<SalesAnalysisRowDto[]>([]);
  prevPeriodRows = signal<SalesAnalysisRowDto[]>([]);
  customers = signal<CustomerDto[]>([]);
  customersById = computed(() => new Map(this.customers().map((c) => [c.customerId, c])));

  selectedGroupKey = signal<string | null>(null);

  groupedRows = computed<GroupRow[]>(() => {
    const rows = this.rows();
    const groupBy = this.groupBy();
    const prevByProduct = this.prevAvgPriceByProduct();

    const groups = new Map<string, { label: string; rows: SalesAnalysisRowDto[] }>();
    for (const row of rows) {
      const [key, label] = this.groupKeyAndLabel(row, groupBy);
      const existing = groups.get(key);
      if (existing) existing.rows.push(row);
      else groups.set(key, { label, rows: [row] });
    }

    const result: GroupRow[] = [];
    for (const [key, { label, rows: groupRows }] of groups) {
      const qty = sum(groupRows, (r) => r.qty);
      const revenue = sum(groupRows, (r) => r.lineTotal);
      const discount = sum(groupRows, (r) => r.discountAmount);
      const cost = sum(groupRows, (r) => r.costAtSale * r.qty);
      result.push({
        key,
        label,
        qty,
        revenue,
        discount,
        cost,
        margin: revenue - cost,
        avgPricePerKg: qty > 0 ? revenue / qty : null,
        prevAvgPricePerKg: groupBy === 'product' ? (prevByProduct.get(key) ?? null) : null,
      });
    }
    return result.sort((a, b) => b.revenue - a.revenue);
  });

  discountsByReason = computed<DiscountReasonRow[]>(() => {
    const byReason = new Map<string, { amount: number; count: number }>();
    for (const row of this.rows()) {
      if (row.discountAmount <= 0) continue;
      const reason = row.discountReason?.trim() || '(no reason given)';
      const existing = byReason.get(reason);
      if (existing) {
        existing.amount += row.discountAmount;
        existing.count += 1;
      } else {
        byReason.set(reason, { amount: row.discountAmount, count: 1 });
      }
    }
    return [...byReason.entries()]
      .map(([reason, v]) => ({ reason, amount: v.amount, count: v.count }))
      .sort((a, b) => b.amount - a.amount);
  });

  detailRowsForSelectedGroup = computed<SalesAnalysisRowDto[]>(() => {
    const key = this.selectedGroupKey();
    if (key === null) return [];
    const groupBy = this.groupBy();
    return this.rows()
      .filter((row) => this.groupKeyAndLabel(row, groupBy)[0] === key)
      .sort((a, b) => b.saleDateTime.localeCompare(a.saleDateTime));
  });

  totalRevenue = computed(() => sum(this.rows(), (r) => r.lineTotal));
  totalQty = computed(() => sum(this.rows(), (r) => r.qty));

  private prevAvgPriceByProduct = computed(() => {
    const map = new Map<string, number>();
    const byProduct = new Map<string, { qty: number; revenue: number }>();
    for (const row of this.prevPeriodRows()) {
      const key = `product:${row.productId}`;
      const existing = byProduct.get(key);
      if (existing) {
        existing.qty += row.qty;
        existing.revenue += row.lineTotal;
      } else {
        byProduct.set(key, { qty: row.qty, revenue: row.lineTotal });
      }
    }
    for (const [key, v] of byProduct) {
      if (v.qty > 0) map.set(key, v.revenue / v.qty);
    }
    return map;
  });

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const from = params.get('from');
    const to = params.get('to');
    if (from) this.from.set(from);
    if (to) this.to.set(to);

    this.customersApi.getAll(true).subscribe((customers) => this.customers.set(customers));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.selectedGroupKey.set(null);
    const from = this.from();
    const to = this.to();
    const prev = precedingPeriod(from, to);

    forkJoin({
      rows: this.api.getSalesAnalysis(from, to),
      prevRows: this.groupBy() === 'product' ? this.api.getSalesAnalysis(prev.from, prev.to) : of([]),
    }).subscribe({
      next: ({ rows, prevRows }) => {
        this.rows.set(rows);
        this.prevPeriodRows.set(prevRows);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load sales analysis. Please try again.');
        this.loading.set(false);
      },
    });
  }

  onGroupByChange(value: GroupBy): void {
    this.groupBy.set(value);
    this.selectedGroupKey.set(null);
    // Only fetch the comparison period's data when it's actually used (product grouping) -
    // re-run load() so the prevPeriodRows forkJoin branch picks it up.
    if (value === 'product' && this.prevPeriodRows().length === 0) this.load();
  }

  toggleGroup(key: string): void {
    this.selectedGroupKey.set(this.selectedGroupKey() === key ? null : key);
  }

  customerName(customerId: number | null): string {
    if (customerId === null) return 'Walk-in';
    return this.customersById().get(customerId)?.name ?? `#${customerId}`;
  }

  private groupKeyAndLabel(row: SalesAnalysisRowDto, groupBy: GroupBy): [string, string] {
    switch (groupBy) {
      case 'product':
        return [`product:${row.productId}`, row.productName];
      case 'grade':
        return [`grade:${row.gradeId ?? 'none'}`, row.gradeName ?? '(no grade)'];
      case 'channel':
        return [`channel:${row.channel}`, row.channel];
      case 'customer':
        return [`customer:${row.customerId ?? 'none'}`, this.customerName(row.customerId)];
      case 'dayOfWeek': {
        const day = new Date(row.saleDateTime).getDay();
        return [`day:${day}`, DAY_NAMES[day]];
      }
    }
  }
}

function sum<T>(items: T[], selector: (item: T) => number): number {
  return items.reduce((acc, item) => acc + selector(item), 0);
}
