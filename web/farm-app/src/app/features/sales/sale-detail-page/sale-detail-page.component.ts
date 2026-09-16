import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CustomerDto } from '../../customers/customer.model';
import { CustomersApiService } from '../../customers/customers-api.service';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { PackSizeDto } from '../../pack-sizes/pack-size.model';
import { PackSizesApiService } from '../../pack-sizes/pack-sizes-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { SaleDto } from '../sale.model';
import { SalesApiService } from '../sales-api.service';

function round2(n: number): number {
  return Math.round((n + Number.EPSILON) * 100) / 100;
}

/**
 * Sale detail (Phase 5d-2): lines, payments, and status at a glance, plus (for a Complete sale)
 * the refund action - see the refund section below the view helpers.
 */
@Component({
  selector: 'app-sale-detail-page',
  imports: [DatePipe, DecimalPipe, FormsModule, RouterLink],
  templateUrl: './sale-detail-page.component.html',
  styleUrl: './sale-detail-page.component.scss',
})
export class SaleDetailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private salesApi = inject(SalesApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  private packSizesApi = inject(PackSizesApiService);
  private customersApi = inject(CustomersApiService);

  loading = signal(true);
  notFound = signal(false);
  sale = signal<SaleDto | null>(null);

  // Includes inactive rows too, same "still resolves, not still selectable" convention as every
  // other cross-entity lookup in this app (see CultivarPage).
  products = signal<ProductDto[]>([]);
  productsById = computed(() => new Map(this.products().map((p) => [p.productId, p])));
  grades = signal<GradeDto[]>([]);
  gradesById = computed(() => new Map(this.grades().map((g) => [g.gradeId, g])));
  packSizes = signal<PackSizeDto[]>([]);
  packSizesById = computed(() => new Map(this.packSizes().map((p) => [p.packSizeId, p])));
  customers = signal<CustomerDto[]>([]);
  customersById = computed(() => new Map(this.customers().map((c) => [c.customerId, c])));

  saleTotal = computed(() => {
    const sale = this.sale();
    if (!sale) return 0;
    return round2(sale.lines.reduce((sum, l) => sum + (l.qty * l.unitPrice - l.discountAmount), 0));
  });

  paymentsTotal = computed(() => {
    const sale = this.sale();
    if (!sale) return 0;
    return round2(sale.payments.reduce((sum, p) => sum + p.amount, 0));
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
      sale: this.salesApi.getById(id),
      products: this.productsApi.getAll(true),
      grades: this.gradesApi.getAll(true),
      packSizes: this.packSizesApi.getAll(),
      customers: this.customersApi.getAll(true),
    }).subscribe({
      next: ({ sale, products, grades, packSizes, customers }) => {
        this.sale.set(sale);
        this.products.set(products);
        this.grades.set(grades);
        this.packSizes.set(packSizes);
        this.customers.set(customers);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  productName(id: number): string {
    const product = this.productsById().get(id);
    if (!product) return `#${id}`;
    return product.isActive ? product.name : `${product.name} (inactive)`;
  }

  gradeName(id: number | null): string {
    if (id === null) return '-';
    const grade = this.gradesById().get(id);
    if (!grade) return `#${id}`;
    return grade.isActive ? grade.name : `${grade.name} (inactive)`;
  }

  packSizeName(id: number | null): string {
    if (id === null) return 'loose';
    return this.packSizesById().get(id)?.name ?? `#${id}`;
  }

  customerName(id: number | null): string {
    if (id === null) return 'Walk-in (no customer)';
    return this.customersById().get(id)?.name ?? `#${id}`;
  }

  // --- Refund (Phase 5d-2) ---

  // Two-step reveal (like the POS product picker's overlay) rather than a single confirm() - a
  // refund is effectively irreversible from the cashier's point of view (task brief: "make sure
  // they can't fat-finger it"), so a bare confirm() dialog with one click through it isn't enough
  // friction. Opening the panel doesn't refund anything by itself; only "Confirm refund" below
  // does.
  confirmingRefund = signal(false);
  refundReason = signal<string | null>(null);
  refunding = signal(false);
  refundError = signal('');

  startRefund(): void {
    this.confirmingRefund.set(true);
    this.refundReason.set(null);
    this.refundError.set('');
  }

  cancelRefund(): void {
    this.confirmingRefund.set(false);
  }

  confirmRefund(): void {
    const sale = this.sale();
    if (!sale || sale.status !== 'Complete') return;

    this.refunding.set(true);
    this.refundError.set('');
    this.salesApi.refund(sale.saleId, { reason: this.refundReason() }).subscribe({
      next: (updated) => {
        this.refunding.set(false);
        this.confirmingRefund.set(false);
        this.sale.set(updated);
      },
      error: (err: HttpErrorResponse) => {
        this.refunding.set(false);
        this.refundError.set(extractErrorMessage(err));
      },
    });
  }
}
