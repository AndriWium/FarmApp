import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { LocationDto } from '../../locations/location.model';
import { LocationsApiService } from '../../locations/locations-api.service';
import { SaleDto } from '../../sales/sale.model';
import { SalesApiService } from '../../sales/sales-api.service';
import { TillSessionDto } from '../till-session.model';
import { TillSessionsApiService } from '../till-sessions-api.service';

function round2(n: number): number {
  return Math.round((n + Number.EPSILON) * 100) / 100;
}

/**
 * Day close (doc 01 Module 4 / doc 02, Phase 5d-2): the operational counterpart to Phase 4c's
 * till-session reporting view. Reached from the POS session bar for the session currently in use
 * - not a standalone list of every session, since a cashier only ever needs to close the one
 * they're sitting at (see DECISIONS.md).
 */
@Component({
  selector: 'app-till-close-page',
  imports: [DatePipe, DecimalPipe, FormsModule, RouterLink],
  templateUrl: './till-close-page.component.html',
  styleUrl: './till-close-page.component.scss',
})
export class TillClosePageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private tillSessionsApi = inject(TillSessionsApiService);
  private salesApi = inject(SalesApiService);
  private locationsApi = inject(LocationsApiService);

  loading = signal(true);
  notFound = signal(false);
  session = signal<TillSessionDto | null>(null);
  sales = signal<SaleDto[]>([]);
  locations = signal<LocationDto[]>([]);
  locationsById = computed(() => new Map(this.locations().map((l) => [l.locationId, l])));

  isClosed = computed(() => this.session()?.closedAt !== null && this.session() !== null);

  // Template-friendly, null-safe views of the closed session's own figures - avoids TS non-null
  // assertions on `session()?.difference` inside the @else-if(isClosed) branch of the template.
  closedDifference = computed(() => this.session()?.difference ?? 0);

  // Refunded sales don't contribute to the card total (matches
  // ISalePaymentRepository.GetCardTotalForTillSessionAsync's own Complete-only filter) but are
  // still shown in the "sales so far" list so a cashier can see the full picture of the session.
  completeSales = computed(() => this.sales().filter((s) => s.status === 'Complete'));

  saleTotal(sale: SaleDto): number {
    return round2(sale.lines.reduce((sum, l) => sum + (l.qty * l.unitPrice - l.discountAmount), 0));
  }

  salesTotal = computed(() => round2(this.completeSales().reduce((sum, s) => sum + this.saleTotal(s), 0)));

  // Client-side preview only, shown before close - the authoritative SystemCardTotal comes back
  // from the close endpoint's response and is never computed here for that purpose.
  cardTotalSoFar = computed(() =>
    round2(
      this.completeSales().reduce(
        (sum, s) => sum + s.payments.filter((p) => p.method === 'Card').reduce((pSum, p) => pSum + p.amount, 0),
        0,
      ),
    ),
  );

  // --- Close form ---

  cardMachineBatchTotal = signal<number | null>(null);
  differenceNote = signal<string | null>(null);
  closing = signal(false);
  closeError = signal('');

  canClose = computed(() => {
    const total = this.cardMachineBatchTotal();
    return total !== null && total >= 0;
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      if (!id) {
        this.notFound.set(true);
        this.loading.set(false);
        return;
      }
      this.load(id);
    });
  }

  private load(id: number): void {
    this.loading.set(true);
    this.notFound.set(false);
    forkJoin({
      session: this.tillSessionsApi.getById(id),
      sales: this.salesApi.getAll(id),
      locations: this.locationsApi.getAll(true),
    }).subscribe({
      next: ({ session, sales, locations }) => {
        this.session.set(session);
        this.sales.set(sales);
        this.locations.set(locations);
        this.loading.set(false);
        // Pre-fill with the system's own running total - the overwhelmingly common case is the
        // card machine agreeing exactly, so this saves the cashier retyping it; any real
        // difference is still a deliberate edit away, not hidden.
        this.cardMachineBatchTotal.set(this.cardTotalSoFar());
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  locationName(id: number): string {
    return this.locationsById().get(id)?.name ?? `#${id}`;
  }

  close(): void {
    const session = this.session();
    const total = this.cardMachineBatchTotal();
    if (!session || total === null || total < 0) return;

    this.closing.set(true);
    this.closeError.set('');
    this.tillSessionsApi
      .close(session.tillSessionId, { cardMachineBatchTotal: round2(total), differenceNote: this.differenceNote() || null })
      .subscribe({
        next: (updated) => {
          this.closing.set(false);
          this.session.set(updated);
        },
        error: (err: HttpErrorResponse) => {
          this.closing.set(false);
          this.closeError.set(extractErrorMessage(err));
        },
      });
  }
}
