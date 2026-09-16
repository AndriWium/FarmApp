import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
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
import { CountLineRequest, StockTakeDto, StockTakeLineDto } from '../stock-take.model';
import { StockTakesApiService } from '../stock-takes-api.service';

@Component({
  selector: 'app-stock-take-detail-page',
  imports: [ReactiveFormsModule, RouterLink, DatePipe, DecimalPipe],
  templateUrl: './stock-take-detail-page.component.html',
  styleUrl: './stock-take-detail-page.component.scss',
})
export class StockTakeDetailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);
  private api = inject(StockTakesApiService);
  private batchesApi = inject(StockBatchesApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  private locationsApi = inject(LocationsApiService);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  stockTakeId = 0;
  loading = signal(true);
  notFound = signal(false);
  stockTake = signal<StockTakeDto | null>(null);

  batches = signal<StockBatchDto[]>([]);
  batchesById = computed(() => new Map(this.batches().map((b) => [b.stockBatchId, b])));
  products = signal<ProductDto[]>([]);
  productsById = computed(() => new Map(this.products().map((p) => [p.productId, p])));
  grades = signal<GradeDto[]>([]);
  gradesById = computed(() => new Map(this.grades().map((g) => [g.gradeId, g])));
  locations = signal<LocationDto[]>([]);

  // Lines already counted (CountedQty !== null) are locked read-only - resubmitting one would
  // make the backend record a second Adjustment for the same batch, double-adjusting on-hand
  // (RecordCountsAsync recomputes Variance and re-fires RecordBatchAdjustmentAsync for every
  // line it's given, with no idempotency check). Only still-pending lines get an editable
  // control, which is also what makes "resume counting later" safe.
  countedLines = computed(() => this.stockTake()?.lines.filter((l) => l.countedQty !== null) ?? []);
  pendingLines = computed(() => this.stockTake()?.lines.filter((l) => l.countedQty === null) ?? []);

  countForm = this.fb.array<ReturnType<typeof this.newCountGroup>>([]);

  submitting = signal(false);
  submitError = signal('');

  ngOnInit(): void {
    this.locationsApi.getAll(true).subscribe((rows) => this.locations.set(rows));
    this.productsApi.getAll(true).subscribe((rows) => this.products.set(rows));
    this.gradesApi.getAll(true).subscribe((rows) => this.grades.set(rows));

    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      this.stockTakeId = id;
      if (!id) {
        this.notFound.set(true);
        this.loading.set(false);
        return;
      }
      this.load();
    });
  }

  private load(): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.submitError.set('');

    forkJoin({
      stockTake: this.api.getById(this.stockTakeId),
      batches: this.batchesApi.getAll(),
    }).subscribe({
      next: ({ stockTake, batches }) => {
        this.stockTake.set(stockTake);
        this.batches.set(batches);
        this.rebuildForm();
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  private newCountGroup(line: StockTakeLineDto) {
    return this.fb.nonNullable.group({
      stockTakeLineId: this.fb.nonNullable.control(line.stockTakeLineId),
      systemQty: this.fb.nonNullable.control(line.systemQty),
      countedQty: this.fb.control<number | null>(null, [Validators.required, Validators.min(0)]),
    });
  }

  private rebuildForm(): void {
    this.countForm.clear();
    for (const line of this.pendingLines()) {
      this.countForm.push(this.newCountGroup(line));
    }
  }

  batchLabel(stockBatchId: number): string {
    const batch = this.batchesById().get(stockBatchId);
    if (!batch) return `Batch #${stockBatchId}`;
    return `${this.productName(batch.productId)}${batch.gradeId !== null ? ' (' + this.gradeName(batch.gradeId) + ')' : ''} - batch #${stockBatchId}`;
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

  locationName(id: number | null): string {
    if (id === null) return '-';
    const location = this.locations().find((l) => l.locationId === id);
    if (!location) return `#${id}`;
    return location.isActive ? location.name : `${location.name} (inactive)`;
  }

  // Let the user count only a few of the pending batches now and come back for the rest later -
  // only rows with a value typed in are sent.
  filledCount(): number {
    return this.countForm.controls.filter((c) => c.getRawValue().countedQty !== null).length;
  }

  submit(): void {
    const counts: CountLineRequest[] = this.countForm.controls
      .map((c) => c.getRawValue())
      .filter((raw): raw is { stockTakeLineId: number; systemQty: number; countedQty: number } => raw.countedQty !== null)
      .map((raw) => ({ stockTakeLineId: raw.stockTakeLineId, countedQty: raw.countedQty }));

    if (counts.length === 0) return;

    this.submitting.set(true);
    this.submitError.set('');

    this.api.recordCounts(this.stockTakeId, { counts }).subscribe({
      next: (stockTake) => {
        this.submitting.set(false);
        this.stockTake.set(stockTake);
        this.rebuildForm();
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.submitError.set(extractErrorMessage(err));
      },
    });
  }
}
