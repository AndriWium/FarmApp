import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { currentMonthRange } from '../../../shared/date-range.util';
import { LocationDto } from '../../locations/location.model';
import { LocationsApiService } from '../../locations/locations-api.service';
import { CashFlowDto, DebtorAgingRowDto, TillSessionOverShortDto } from '../report.model';
import { ReportsApiService } from '../reports-api.service';

/**
 * Cash & debtors report (doc 04 §5): till session over/short (colour-coded per session), cash
 * flow summary for a date range, and debtors aging as of today (current/30/60/90+, oldest buckets
 * highlighted - doc's "colour/highlight anything in the older buckets"). Till sessions are
 * drillable straight to that session's sales via SaleListPageComponent's tillSessionId query param
 * (added alongside this report - see SaleListPageComponent's own comment).
 */
@Component({
  selector: 'app-cash-debtors-page',
  imports: [DatePipe, DecimalPipe, FormsModule, RouterLink],
  templateUrl: './cash-debtors-page.component.html',
  styleUrl: './cash-debtors-page.component.scss',
})
export class CashDebtorsPageComponent implements OnInit {
  private api = inject(ReportsApiService);
  private locationsApi = inject(LocationsApiService);

  private defaultRange = currentMonthRange();
  from = signal(this.defaultRange.from);
  to = signal(this.defaultRange.to);

  locations = signal<LocationDto[]>([]);
  locationsById = computed(() => new Map(this.locations().map((l) => [l.locationId, l])));

  loading = signal(true);
  error = signal('');

  tillSessions = signal<TillSessionOverShortDto[]>([]);
  cashFlow = signal<CashFlowDto | null>(null);
  debtors = signal<DebtorAgingRowDto[]>([]);

  sortedTillSessions = computed(() => [...this.tillSessions()].sort((a, b) => b.closedAt.localeCompare(a.closedAt)));

  totalDifference = computed(() => this.tillSessions().reduce((sum, s) => sum + (s.difference ?? 0), 0));

  totalDebtors = computed(() => this.debtors().reduce((sum, d) => sum + d.total, 0));
  totalDays60Plus = computed(() => this.debtors().reduce((sum, d) => sum + d.days60 + d.days90Plus, 0));

  ngOnInit(): void {
    this.locationsApi.getAll(true).subscribe((locations) => this.locations.set(locations));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    forkJoin({
      tillSessions: this.api.getTillSessions(this.from(), this.to()),
      cashFlow: this.api.getCashFlow(this.from(), this.to()),
      debtors: this.api.getDebtorsAging(),
    }).subscribe({
      next: ({ tillSessions, cashFlow, debtors }) => {
        this.tillSessions.set(tillSessions);
        this.cashFlow.set(cashFlow);
        this.debtors.set(debtors);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load cash & debtors reports. Please try again.');
        this.loading.set(false);
      },
    });
  }

  locationName(id: number): string {
    return this.locationsById().get(id)?.name ?? `#${id}`;
  }
}
