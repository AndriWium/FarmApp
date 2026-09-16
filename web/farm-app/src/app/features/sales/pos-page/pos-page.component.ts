import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { extractErrorMessage } from '../../../shared/http-error.util';
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
import { TillSessionDto } from '../../till-sessions/till-session.model';
import { TillSessionsApiService } from '../../till-sessions/till-sessions-api.service';

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
  priceLists = signal<PriceListDto[]>([]);

  // "Retail" if one exists (the overwhelmingly common sale), else whatever's first active - a
  // judgment call since the task brief only said "whichever makes sense" (see DECISIONS.md).
  priceListId = signal<number | null>(null);

  private loadCatalog(): void {
    this.productsApi.getAll().subscribe((rows) => this.products.set(rows));
    this.gradesApi.getAll().subscribe((rows) => this.grades.set(rows));
    this.packSizesApi.getAll().subscribe((rows) => this.packSizes.set(rows));
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
    this.closePicker();
  }

  // --- Basket ---

  basket = signal<BasketLine[]>([]);

  lineTotal(line: BasketLine): number {
    return line.qty * line.unitPrice - line.discountAmount;
  }

  basketTotal = computed(() => this.basket().reduce((sum, line) => sum + this.lineTotal(line), 0));

  removeLine(key: string): void {
    this.basket.update((lines) => lines.filter((l) => l.key !== key));
  }

  setLineDiscountAmount(key: string, amount: number): void {
    this.basket.update((lines) =>
      lines.map((l) => (l.key === key ? { ...l, discountAmount: Number.isFinite(amount) && amount >= 0 ? amount : 0 } : l)),
    );
  }

  setLineDiscountReason(key: string, reason: string): void {
    this.basket.update((lines) => lines.map((l) => (l.key === key ? { ...l, discountReason: reason || null } : l)));
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
