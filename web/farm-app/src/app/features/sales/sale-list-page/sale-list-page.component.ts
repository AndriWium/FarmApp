import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { LocationDto } from '../../locations/location.model';
import { LocationsApiService } from '../../locations/locations-api.service';
import { TillSessionDto } from '../../till-sessions/till-session.model';
import { TillSessionsApiService } from '../../till-sessions/till-sessions-api.service';
import { SaleDto } from '../sale.model';
import { SalesApiService } from '../sales-api.service';

function round2(n: number): number {
  return Math.round((n + Number.EPSILON) * 100) / 100;
}

/**
 * Sale history / lookup (Phase 5d-2). The till-session filter is server-side (the only filter
 * SalesController.GetAll supports); the date range is client-side over whatever that call
 * returns - there's no date param on the API and this app's sale volume doesn't warrant adding
 * one yet (see DECISIONS.md).
 */
@Component({
  selector: 'app-sale-list-page',
  imports: [DatePipe, DecimalPipe, FormsModule, RouterLink],
  templateUrl: './sale-list-page.component.html',
  styleUrl: './sale-list-page.component.scss',
})
export class SaleListPageComponent implements OnInit {
  private salesApi = inject(SalesApiService);
  private tillSessionsApi = inject(TillSessionsApiService);
  private locationsApi = inject(LocationsApiService);
  private route = inject(ActivatedRoute);

  loading = signal(true);
  sales = signal<SaleDto[]>([]);
  tillSessions = signal<TillSessionDto[]>([]);
  tillSessionsById = computed(() => new Map(this.tillSessions().map((t) => [t.tillSessionId, t])));
  locations = signal<LocationDto[]>([]);
  locationsById = computed(() => new Map(this.locations().map((l) => [l.locationId, l])));

  tillSessionFilter = signal<number | null>(null);
  dateFrom = signal<string | null>(null); // yyyy-MM-dd, inclusive
  dateTo = signal<string | null>(null); // yyyy-MM-dd, inclusive

  filteredSales = computed(() => {
    const from = this.dateFrom();
    const to = this.dateTo();
    return this.sales().filter((s) => {
      const day = s.dateTime.slice(0, 10);
      if (from && day < from) return false;
      if (to && day > to) return false;
      return true;
    });
  });

  sortedSales = computed(() => [...this.filteredSales()].sort((a, b) => b.dateTime.localeCompare(a.dateTime)));

  ngOnInit(): void {
    // Supports deep-linking from the Cash & Debtors report's till-session over/short list (doc
    // 04's drillability principle: "click a total -> see the transactions") - a ?tillSessionId=
    // query param preselects the same server-side filter the dropdown itself sets.
    const tillSessionIdParam = this.route.snapshot.queryParamMap.get('tillSessionId');
    if (tillSessionIdParam) this.tillSessionFilter.set(Number(tillSessionIdParam));

    this.loadLookups();
    this.load();
  }

  private loadLookups(): void {
    forkJoin({
      tillSessions: this.tillSessionsApi.getAll(null, false),
      locations: this.locationsApi.getAll(true),
    }).subscribe(({ tillSessions, locations }) => {
      this.tillSessions.set(tillSessions);
      this.locations.set(locations);
    });
  }

  load(): void {
    this.loading.set(true);
    this.salesApi.getAll(this.tillSessionFilter()).subscribe((rows) => {
      this.sales.set(rows);
      this.loading.set(false);
    });
  }

  onTillSessionFilterChange(value: string): void {
    this.tillSessionFilter.set(value === '' ? null : Number(value));
    this.load();
  }

  saleTotal(sale: SaleDto): number {
    return round2(sale.lines.reduce((sum, l) => sum + (l.qty * l.unitPrice - l.discountAmount), 0));
  }

  tillSessionLabel(id: number): string {
    const session = this.tillSessionsById().get(id);
    if (!session) return `#${id}`;
    const location = this.locationsById().get(session.locationId);
    return `${location?.name ?? '#' + session.locationId} - ${new Date(session.openedAt).toLocaleDateString()}`;
  }
}
