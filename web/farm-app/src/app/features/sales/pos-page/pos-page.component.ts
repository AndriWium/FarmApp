import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CustomerDto } from '../../customers/customer.model';
import { CustomersApiService } from '../../customers/customers-api.service';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { LocationDto } from '../../locations/location.model';
import { LocationsApiService } from '../../locations/locations-api.service';
import { PackSizeDto } from '../../pack-sizes/pack-size.model';
import { PackSizesApiService } from '../../pack-sizes/pack-sizes-api.service';
import { PriceListDto } from '../../price-lists/price-list.model';
import { PriceListsApiService } from '../../price-lists/price-lists-api.service';
import { PricesApiService } from '../../prices/prices-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { CreateSaleRequest, SALE_CHANNELS, SALE_PAYMENT_METHODS, SaleChannel, SaleDto, SalePaymentMethod } from '../sale.model';
import { SalesApiService } from '../sales-api.service';
import { StockMovementsApiService } from '../../stock-movements/stock-movements-api.service';
import { TillSessionDto } from '../../till-sessions/till-session.model';
import { TillSessionsApiService } from '../../till-sessions/till-sessions-api.service';

/** One row of the split-payment editor at checkout - Card/EFT/Account amounts must sum to exactly
 * the basket total (SaleService.CreateSaleAsync's PaymentMismatch check, no partial payments). */
interface PaymentRow {
  method: SalePaymentMethod;
  amount: number;
}

function round2(n: number): number {
  return Math.round((n + Number.EPSILON) * 100) / 100;
}

/** One product/grade/pack-size/qty/price/discount line sitting in the basket, before checkout. A
 * plain signal-backed array rather than a FormArray (task brief's own call) - the basket is built
 * by tapping product cards and filling a small picker, not by typing rows into a table the way
 * ProducePurchaseFormPageComponent's line editor is, so there's no form to bind qty/price/discount
 * inputs to until a line already exists; a FormArray would just be ceremony around that. `key` is
 * a client-only id (crypto.randomUUID()) for @for tracking and removal - never sent to the API. */
interface BasketLine {
  key: string;
  productId: number;
  productName: string;
  gradeId: number | null;
  gradeName: string | null;
  packSizeId: number | null;
  packSizeName: string | null; // null = loose, sold in the product's own base unit
  qty: number; // packs, when packSizeId is set; base units (kg/each) otherwise - same rule as CreateSaleLineRequest
  unitPrice: number;
  discountAmount: number;
  discountReason: string | null;
}

/**
 * The single most important screen in FarmApp: card-only checkout, meant to run on a laptop at a
 * real market stall (doc 01 Module 4). Kept as one component rather than split into a multi-route
 * wizard (unlike stock-takes' start/detail split) - a cashier mid-sale needs the till-session
 * state, product grid, basket, and checkout all reachable without a navigation, per the task
 * brief's "big-button, touch-friendly... used with dirty hands in a hurry" framing.
 */
@Component({
  selector: 'app-pos-page',
  imports: [DatePipe, DecimalPipe, FormsModule],
  templateUrl: './pos-page.component.html',
  styleUrl: './pos-page.component.scss',
})
export class PosPageComponent implements OnInit {
  private tillSessionsApi = inject(TillSessionsApiService);
  private locationsApi = inject(LocationsApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  private packSizesApi = inject(PackSizesApiService);
  private priceListsApi = inject(PriceListsApiService);
  private pricesApi = inject(PricesApiService);
  private customersApi = inject(CustomersApiService);
  private salesApi = inject(SalesApiService);
  private stockMovementsApi = inject(StockMovementsApiService);

  // --- Till-session gate (Phase 5d-1 commit 1) ---

  gateLoading = signal(true);
  gateError = signal('');
  openSessions = signal<TillSessionDto[]>([]);
  locations = signal<LocationDto[]>([]);
  locationsById = computed(() => new Map(this.locations().map((l) => [l.locationId, l])));
  tillSession = signal<TillSessionDto | null>(null);
  openLocationId = signal<number | null>(null);
  opening = signal(false);
  openError = signal('');

  // --- Catalog (products/grades/pack sizes/price lists), loaded once the gate opens ---

  products = signal<ProductDto[]>([]);
  activeProducts = computed(() => this.products().filter((p) => p.isActive));
  grades = signal<GradeDto[]>([]);
  gradesById = computed(() => new Map(this.grades().map((g) => [g.gradeId, g])));
  packSizes = signal<PackSizeDto[]>([]);
  packSizesById = computed(() => new Map(this.packSizes().map((p) => [p.packSizeId, p])));
  priceLists = signal<PriceListDto[]>([]);
  productsById = computed(() => new Map(this.products().map((p) => [p.productId, p])));
  customers = signal<CustomerDto[]>([]);
  activeCustomers = computed(() => this.customers().filter((c) => c.isActive));
  customersById = computed(() => new Map(this.customers().map((c) => [c.customerId, c])));

  // "Retail" if one exists (the overwhelmingly common sale), else whatever's first active - a
  // judgment call since the task brief only said "whichever makes sense" (see DECISIONS.md).
  priceListId = signal<number | null>(null);

  private loadCatalog(): void {
    this.productsApi.getAll().subscribe((rows) => this.products.set(rows));
    this.gradesApi.getAll().subscribe((rows) => this.grades.set(rows));
    this.packSizesApi.getAll().subscribe((rows) => this.packSizes.set(rows));
    this.customersApi.getAll().subscribe((rows) => this.customers.set(rows));
    this.priceListsApi.getAll().subscribe((rows) => {
      this.priceLists.set(rows);
      const retail = rows.find((p) => p.isActive && p.name.toLowerCase().includes('retail'));
      const fallback = rows.find((p) => p.isActive);
      this.priceListId.set((retail ?? fallback)?.priceListId ?? null);
    });
  }

  packSizesFor(productId: number): PackSizeDto[] {
    return this.packSizes().filter((p) => p.productId === productId);
  }

  // --- Product picker overlay: tap a card, choose grade/pack/qty, look up the price ---

  pickerProduct = signal<ProductDto | null>(null);
  pickerGradeId = signal<number | null>(null);
  pickerPackSizeId = signal<number | null>(null); // null = loose, in the product's base unit
  pickerQty = signal<number | null>(null);
  pickerUnitPrice = signal<number | null>(null);
  pickerPriceHint = signal('');
  pickerLoadingPrice = signal(false);

  pickerPackSizes = computed(() => {
    const product = this.pickerProduct();
    return product ? this.packSizesFor(product.productId) : [];
  });
  // Grade only matters for Produce (doc 01's own framing - class 1/2/juicing is a produce concept)
  // - shown for that product type so the cashier isn't asked to pick one for a bag of coffee.
  pickerShowsGrade = computed(() => this.pickerProduct()?.productType === 'Produce');

  openPicker(product: ProductDto): void {
    this.pickerProduct.set(product);
    this.pickerGradeId.set(null);
    this.pickerPackSizeId.set(null);
    this.pickerQty.set(null);
    this.pickerUnitPrice.set(null);
    this.pickerPriceHint.set('');
    this.lookupPickerPrice();
  }

  closePicker(): void {
    this.pickerProduct.set(null);
  }

  onPickerGradeChange(value: string): void {
    this.pickerGradeId.set(value === '' ? null : Number(value));
    this.lookupPickerPrice();
  }

  onPickerPackSizeChange(value: string): void {
    this.pickerPackSizeId.set(value === '' ? null : Number(value));
    this.lookupPickerPrice();
  }

  private lookupPickerPrice(): void {
    const product = this.pickerProduct();
    const priceListId = this.priceListId();
    if (!product || !priceListId) return;

    this.pickerLoadingPrice.set(true);
    this.pickerPriceHint.set('');
    this.pricesApi
      .getCurrent(priceListId, product.productId, this.pickerGradeId(), this.pickerPackSizeId())
      .subscribe((price) => {
        this.pickerLoadingPrice.set(false);
        if (price) {
          this.pickerUnitPrice.set(price.unitPrice);
        } else {
          this.pickerUnitPrice.set(null);
          this.pickerPriceHint.set('No price set for this combination - enter one manually.');
        }
      });
  }

  pickerCanAdd = computed(() => {
    const qty = this.pickerQty();
    const price = this.pickerUnitPrice();
    return this.pickerProduct() !== null && qty !== null && qty > 0 && price !== null && price >= 0;
  });

  addPickerToBasket(): void {
    const product = this.pickerProduct();
    const qty = this.pickerQty();
    const unitPrice = this.pickerUnitPrice();
    if (!product || qty === null || qty <= 0 || unitPrice === null) return;

    const gradeId = this.pickerGradeId();
    const packSizeId = this.pickerPackSizeId();
    const packSize = packSizeId === null ? null : this.packSizes().find((p) => p.packSizeId === packSizeId) ?? null;

    this.ensureClientGuid();
    this.basket.update((lines) => [
      ...lines,
      {
        key: crypto.randomUUID(),
        productId: product.productId,
        productName: product.name,
        gradeId,
        gradeName: gradeId === null ? null : (this.gradesById().get(gradeId)?.name ?? `#${gradeId}`),
        packSizeId,
        packSizeName: packSize?.name ?? null,
        qty,
        unitPrice,
        discountAmount: 0,
        discountReason: null,
      },
    ]);
    this.syncSinglePayment();
    this.closePicker();
  }

  // --- Basket ---

  basket = signal<BasketLine[]>([]);

  lineTotal(line: BasketLine): number {
    return round2(line.qty * line.unitPrice - line.discountAmount);
  }

  basketTotal = computed(() => round2(this.basket().reduce((sum, line) => sum + this.lineTotal(line), 0)));

  removeLine(key: string): void {
    this.basket.update((lines) => lines.filter((l) => l.key !== key));
    this.insufficientStockKeys.update((keys) => {
      if (!keys.has(key)) return keys;
      const next = new Set(keys);
      next.delete(key);
      return next;
    });
    // An emptied basket is a genuinely new "next sale" the moment something is added again - see
    // ensureClientGuid's comment for why this matters.
    if (this.basket().length === 0) this.clientGuid.set(null);
    else this.syncSinglePayment();
  }

  setLineDiscountAmount(key: string, amount: number): void {
    this.basket.update((lines) =>
      lines.map((l) => (l.key === key ? { ...l, discountAmount: Number.isFinite(amount) && amount >= 0 ? amount : 0 } : l)),
    );
    this.syncSinglePayment();
  }

  setLineDiscountReason(key: string, reason: string): void {
    this.basket.update((lines) => lines.map((l) => (l.key === key ? { ...l, discountReason: reason || null } : l)));
  }

  // --- Checkout ---

  // ClientGuid (doc 08): generated once when the basket starts (the first line added to an empty
  // basket - see addPickerToBasket/removeLine above), NOT regenerated on every submit attempt.
  // That's what makes a flaky-connection retry idempotent: resubmitting the same basket sends the
  // same ClientGuid, so the backend either creates the sale once or (on a genuine retry after the
  // first attempt actually landed) returns the existing one instead of creating a second. Only a
  // real success rotates it (see submit()'s next handler) - every error path, network failure
  // included, leaves it untouched on purpose.
  clientGuid = signal<string | null>(null);

  private ensureClientGuid(): void {
    if (!this.clientGuid()) this.clientGuid.set(crypto.randomUUID());
  }

  channel = signal<SaleChannel>('FarmStall');
  saleChannels = SALE_CHANNELS;
  paymentMethods = SALE_PAYMENT_METHODS;
  customerId = signal<number | null>(null);
  notes = signal<string | null>(null);

  paymentRows = signal<PaymentRow[]>([{ method: 'Card', amount: 0 }]);
  paymentsTotal = computed(() => round2(this.paymentRows().reduce((sum, r) => sum + (r.amount || 0), 0)));
  paymentsRemaining = computed(() => round2(this.basketTotal() - this.paymentsTotal()));
  requiresCustomer = computed(() => this.paymentRows().some((r) => r.method === 'Account'));

  // The common case (one payment method covering the whole sale) should never need the cashier to
  // retype the total by hand - keeps the single row's amount tracking the basket as it changes.
  // The moment a second row exists, amounts become the cashier's own call (a real split).
  private syncSinglePayment(): void {
    if (this.paymentRows().length === 1) {
      this.paymentRows.set([{ ...this.paymentRows()[0], amount: this.basketTotal() }]);
    }
  }

  addPaymentRow(): void {
    const usedMethods = new Set(this.paymentRows().map((r) => r.method));
    const nextMethod = this.paymentMethods.find((m) => !usedMethods.has(m)) ?? 'Card';
    const remaining = Math.max(this.paymentsRemaining(), 0);
    this.paymentRows.update((rows) => [...rows, { method: nextMethod, amount: remaining }]);
  }

  removePaymentRow(index: number): void {
    if (this.paymentRows().length <= 1) return;
    this.paymentRows.update((rows) => rows.filter((_, i) => i !== index));
  }

  setPaymentMethod(index: number, method: SalePaymentMethod): void {
    this.paymentRows.update((rows) => rows.map((r, i) => (i === index ? { ...r, method } : r)));
  }

  setPaymentAmount(index: number, amount: number): void {
    this.paymentRows.update((rows) =>
      rows.map((r, i) => (i === index ? { ...r, amount: Number.isFinite(amount) && amount >= 0 ? amount : 0 } : r)),
    );
  }

  canSubmit = computed(() => {
    if (this.basket().length === 0) return false;
    if (this.paymentRows().some((r) => r.amount <= 0)) return false;
    if (this.paymentsRemaining() !== 0) return false;
    if (this.requiresCustomer() && this.customerId() === null) return false;
    return true;
  });

  submitting = signal(false);
  submitError = signal('');
  insufficientStockKeys = signal<Set<string>>(new Set());
  lastSale = signal<SaleDto | null>(null);
  lastSaleWasReplay = signal(false);

  submit(): void {
    const session = this.tillSession();
    if (!this.canSubmit() || !session) return;

    this.ensureClientGuid();
    this.submitting.set(true);
    this.submitError.set('');
    this.insufficientStockKeys.set(new Set());

    const request: CreateSaleRequest = {
      clientGuid: this.clientGuid()!,
      tillSessionId: session.tillSessionId,
      customerId: this.customerId(),
      channel: this.channel(),
      notes: this.notes(),
      lines: this.basket().map((l) => ({
        productId: l.productId,
        gradeId: l.gradeId,
        packSizeId: l.packSizeId,
        qty: l.qty,
        unitPrice: l.unitPrice,
        discountAmount: l.discountAmount,
        discountReason: l.discountReason,
      })),
      payments: this.paymentRows().map((p) => ({ method: p.method, amount: round2(p.amount) })),
    };

    this.salesApi.create(request).subscribe({
      next: (res) => {
        this.submitting.set(false);
        this.lastSale.set(res.body ?? null);
        // doc 08: 201 is a genuine first create, 200 is an idempotent replay of an already-landed
        // sale (only reachable here if a prior attempt's response was lost but the write stuck).
        this.lastSaleWasReplay.set(res.status === 200);
        this.basket.set([]);
        this.paymentRows.set([{ method: 'Card', amount: 0 }]);
        this.customerId.set(null);
        this.notes.set(null);
        this.channel.set('FarmStall');
        // A real success is the only thing that rotates the ClientGuid - see its own comment.
        this.clientGuid.set(null);
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        // status 0: the request never reached the server (offline/refused/timed out) - nothing was
        // created, so the same ClientGuid is safe and correct to resubmit unchanged (doc 08).
        if (err.status === 0) {
          this.submitError.set(
            'Could not reach the server. Your basket is safe - check the connection and tap "Submit sale" again.',
          );
          return;
        }

        const title = (err.error as { title?: string } | null)?.title;
        if (err.status === 409 && title === 'Insufficient stock') {
          this.submitError.set(extractErrorMessage(err));
          this.flagShortLines();
          return;
        }

        this.submitError.set(extractErrorMessage(err));
      },
    });
  }

  // Best-effort "which line was short" highlight: SaleService's InsufficientStock detail message
  // (StockMovementService.TryAllocateAsync) reports Requested/Available qty but not which basket
  // line triggered it, so this re-fetches on-hand and re-walks the basket in the same order the
  // backend allocates (product/grade, cumulative) to find the first line(s) that would overrun -
  // matches doc 01's "show which product/grade was short" without needing a backend change.
  private flagShortLines(): void {
    this.stockMovementsApi.getOnHandSummary().subscribe((summary) => {
      const available = new Map<string, number>();
      for (const row of summary) available.set(`${row.productId}|${row.gradeId ?? 'null'}`, row.qtyOnHand);

      const used = new Map<string, number>();
      const shortKeys = new Set<string>();
      for (const line of this.basket()) {
        const packSize = line.packSizeId === null ? null : this.packSizesById().get(line.packSizeId);
        const baseQty = packSize ? line.qty * packSize.qtyInBaseUnit : line.qty;
        const mapKey = `${line.productId}|${line.gradeId ?? 'null'}`;
        const usedSoFar = used.get(mapKey) ?? 0;
        const onHand = available.get(mapKey) ?? 0;
        if (usedSoFar + baseQty > onHand) shortKeys.add(line.key);
        used.set(mapKey, usedSoFar + baseQty);
      }
      this.insufficientStockKeys.set(shortKeys);
    });
  }

  dismissReceipt(): void {
    this.lastSale.set(null);
  }

  saleLineProductName(productId: number): string {
    return this.productsById().get(productId)?.name ?? `#${productId}`;
  }

  saleLineGradeName(gradeId: number | null): string {
    if (gradeId === null) return '-';
    return this.gradesById().get(gradeId)?.name ?? `#${gradeId}`;
  }

  saleLinePackSizeName(packSizeId: number | null): string {
    if (packSizeId === null) return 'loose';
    return this.packSizesById().get(packSizeId)?.name ?? `#${packSizeId}`;
  }

  customerName(id: number | null): string {
    if (id === null) return 'Walk-in (no customer)';
    return this.customersById().get(id)?.name ?? `#${id}`;
  }

  saleTotal(sale: SaleDto): number {
    return round2(sale.lines.reduce((sum, l) => sum + (l.qty * l.unitPrice - l.discountAmount), 0));
  }

  ngOnInit(): void {
    this.locationsApi.getAll().subscribe((rows) => this.locations.set(rows));
    this.loadCatalog();
    this.loadGate();
  }

  private loadGate(): void {
    this.gateLoading.set(true);
    this.gateError.set('');
    this.tillSessionsApi.getAll(null, true).subscribe({
      next: (sessions) => {
        this.gateLoading.set(false);
        this.openSessions.set(sessions);
        // Exactly one open session anywhere - the overwhelmingly common real case (one cashier,
        // one stall) - skips straight to the sell screen with no extra tap.
        if (sessions.length === 1) this.tillSession.set(sessions[0]);
      },
      error: (err: HttpErrorResponse) => {
        this.gateLoading.set(false);
        this.gateError.set(extractErrorMessage(err));
      },
    });
  }

  locationName(id: number): string {
    return this.locationsById().get(id)?.name ?? `#${id}`;
  }

  useSession(session: TillSessionDto): void {
    this.tillSession.set(session);
  }

  openSession(): void {
    const locationId = this.openLocationId();
    if (!locationId) return;

    this.opening.set(true);
    this.openError.set('');
    this.tillSessionsApi.open({ locationId }).subscribe({
      next: (session) => {
        this.opening.set(false);
        this.tillSession.set(session);
      },
      error: (err: HttpErrorResponse) => {
        this.opening.set(false);
        this.openError.set(extractErrorMessage(err));
      },
    });
  }

  // Lets a cashier back out of the sell screen to switch sessions (e.g. picked the wrong location)
  // without a page reload - re-checks what's actually open rather than trusting stale state.
  changeSession(): void {
    this.tillSession.set(null);
    this.loadGate();
  }
}
