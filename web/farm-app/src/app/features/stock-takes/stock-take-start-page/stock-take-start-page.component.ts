import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, map, of, switchMap } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { LocationDto } from '../../locations/location.model';
import { LocationsApiService } from '../../locations/locations-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { StockBatchDto } from '../../stock-batches/stock-batch.model';
import { StockBatchesApiService } from '../../stock-batches/stock-batches-api.service';
import { StockTakesApiService } from '../stock-takes-api.service';

interface BatchRow extends StockBatchDto {
  onHand: number;
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

@Component({
  selector: 'app-stock-take-start-page',
  imports: [ReactiveFormsModule, RouterLink, DatePipe, DecimalPipe],
  templateUrl: './stock-take-start-page.component.html',
  styleUrl: './stock-take-start-page.component.scss',
})
export class StockTakeStartPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private batchesApi = inject(StockBatchesApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  private locationsApi = inject(LocationsApiService);
  private stockTakesApi = inject(StockTakesApiService);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  loading = signal(true);
  rows = signal<BatchRow[]>([]);
  products = signal<ProductDto[]>([]);
  productsById = computed(() => new Map(this.products().map((p) => [p.productId, p])));
  grades = signal<GradeDto[]>([]);
  gradesById = computed(() => new Map(this.grades().map((g) => [g.gradeId, g])));
  locations = signal<LocationDto[]>([]);

  // Zero-on-hand batches are hidden by default, same convention as the stock-on-hand screen - a
  // batch you have none of isn't a meaningful thing to count.
  showDepleted = signal(false);
  visibleRows = computed(() => {
    const rows = this.rows();
    return this.showDepleted() ? rows : rows.filter((r) => r.onHand > 0);
  });

  selectedIds = signal<Set<number>>(new Set());

  header = this.fb.nonNullable.group({
    date: [todayIso(), [Validators.required]],
    locationId: this.fb.control<number | null>(null),
    notes: this.fb.control<string | null>(null, [Validators.maxLength(500)]),
  });

  submitting = signal(false);
  submitError = signal('');

  ngOnInit(): void {
    this.locationsApi.getAll().subscribe((rows) => this.locations.set(rows));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      products: this.productsApi.getAll(true),
      grades: this.gradesApi.getAll(true),
      batches: this.batchesApi.getAll(),
    })
      .pipe(
        switchMap(({ products, grades, batches }) => {
          this.products.set(products);
          this.grades.set(grades);
          if (batches.length === 0) return of({ batches, onHands: [] as number[] });
          return forkJoin(batches.map((b) => this.batchesApi.getOnHand(b.stockBatchId))).pipe(
            map((onHands) => ({ batches, onHands })),
          );
        }),
      )
      .subscribe(({ batches, onHands }) => {
        const rows = batches.map((b, i) => ({ ...b, onHand: onHands[i] }));
        rows.sort((a, b) => a.productId - b.productId || a.date.localeCompare(b.date));
        this.rows.set(rows);
        this.loading.set(false);
      });
  }

  toggleShowDepleted(): void {
    this.showDepleted.update((v) => !v);
  }

  isSelected(id: number): boolean {
    return this.selectedIds().has(id);
  }

  toggleSelected(id: number): void {
    this.selectedIds.update((set) => {
      const next = new Set(set);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  productName(productId: number): string {
    const product = this.productsById().get(productId);
    if (!product) return `#${productId}`;
    return product.isActive ? product.name : `${product.name} (inactive)`;
  }

  gradeName(gradeId: number | null): string {
    if (gradeId === null) return '-';
    const grade = this.gradesById().get(gradeId);
    if (!grade) return `#${gradeId}`;
    return grade.isActive ? grade.name : `${grade.name} (inactive)`;
  }

  submit(): void {
    if (this.header.invalid || this.selectedIds().size === 0) {
      this.header.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set('');

    const raw = this.header.getRawValue();
    this.stockTakesApi
      .start({
        date: raw.date,
        locationId: raw.locationId,
        notes: raw.notes,
        stockBatchIds: [...this.selectedIds()],
      })
      .subscribe({
        next: (stockTake) => {
          this.submitting.set(false);
          this.router.navigate(['/stock-takes', stockTake.stockTakeId]);
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          this.submitError.set(extractErrorMessage(err));
        },
      });
  }
}
