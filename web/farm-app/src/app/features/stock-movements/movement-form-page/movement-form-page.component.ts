import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { LocationDto } from '../../locations/location.model';
import { LocationsApiService } from '../../locations/locations-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import {
  MOVEMENT_KINDS,
  MovementKind,
  SimpleMovementKind,
  StockMovementDto,
} from '../stock-movement.model';
import { StockMovementsApiService } from '../stock-movements-api.service';

@Component({
  selector: 'app-movement-form-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './movement-form-page.component.html',
  styleUrl: './movement-form-page.component.scss',
})
export class MovementFormPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private movementsApi = inject(StockMovementsApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  private locationsApi = inject(LocationsApiService);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  products = signal<ProductDto[]>([]);
  grades = signal<GradeDto[]>([]);
  locations = signal<LocationDto[]>([]);

  kind = signal<MovementKind | null>(null);
  meta = computed(() => MOVEMENT_KINDS.find((m) => m.kind === this.kind()) ?? null);
  isTransfer = computed(() => this.kind() === 'transfer');

  form = this.fb.nonNullable.group({
    productId: [0, [Validators.required, Validators.min(1)]],
    gradeId: this.fb.control<number | null>(null),
    qty: [0, [Validators.required, Validators.min(0.001)]],
    reason: this.fb.control<string | null>(null, [Validators.maxLength(200)]),
    locationId: this.fb.control<number | null>(null),
    fromLocationId: this.fb.control<number | null>(null),
    toLocationId: this.fb.control<number | null>(null),
  });

  submitting = signal(false);
  submitError = signal('');
  insufficientStock = signal(false);
  lastResult = signal<StockMovementDto[] | null>(null);

  ngOnInit(): void {
    this.productsApi.getAll(true).subscribe((rows) => this.products.set(rows));
    this.gradesApi.getAll(true).subscribe((rows) => this.grades.set(rows));
    this.locationsApi.getAll(true).subscribe((rows) => this.locations.set(rows));

    this.route.paramMap.subscribe((params) => {
      const kindParam = params.get('kind') as MovementKind | null;
      const meta = MOVEMENT_KINDS.find((m) => m.kind === kindParam);
      if (!meta) {
        this.router.navigateByUrl('/stock-movements');
        return;
      }
      this.kind.set(meta.kind);
      this.lastResult.set(null);
      this.submitError.set('');
      this.insufficientStock.set(false);
      this.resetForm();
    });
  }

  private resetForm(): void {
    this.form.reset({
      productId: 0,
      gradeId: null,
      qty: 0,
      reason: null,
      locationId: null,
      fromLocationId: null,
      toLocationId: null,
    });

    if (this.isTransfer()) {
      this.form.controls.locationId.disable({ emitEvent: false });
      this.form.controls.fromLocationId.enable({ emitEvent: false });
      this.form.controls.toLocationId.enable({ emitEvent: false });
      this.form.controls.fromLocationId.setValidators([Validators.required, Validators.min(1)]);
      this.form.controls.toLocationId.setValidators([Validators.required, Validators.min(1)]);
      this.form.controls.reason.clearValidators();
      this.form.controls.reason.setValidators([Validators.maxLength(200)]);
    } else {
      this.form.controls.fromLocationId.disable({ emitEvent: false });
      this.form.controls.toLocationId.disable({ emitEvent: false });
      this.form.controls.locationId.enable({ emitEvent: false });
      this.form.controls.fromLocationId.clearValidators();
      this.form.controls.toLocationId.clearValidators();
      // Backend only enforces MaxLength(200) on Reason (not Required) - the task brief asks the UI
      // to require it for these single-location types anyway, since "why" matters for wastage/
      // own-use/etc. record-keeping even though the API itself would accept a blank one.
      this.form.controls.reason.setValidators([Validators.required, Validators.maxLength(200)]);
    }
    this.form.controls.fromLocationId.updateValueAndValidity({ emitEvent: false });
    this.form.controls.toLocationId.updateValueAndValidity({ emitEvent: false });
    this.form.controls.reason.updateValueAndValidity({ emitEvent: false });
  }

  submit(): void {
    const kind = this.kind();
    if (this.form.invalid || !kind) return;

    this.submitting.set(true);
    this.submitError.set('');
    this.insufficientStock.set(false);

    const raw = this.form.getRawValue();
    const obs =
      kind === 'transfer'
        ? this.movementsApi.transfer({
            productId: raw.productId,
            gradeId: raw.gradeId,
            qty: raw.qty,
            fromLocationId: raw.fromLocationId!,
            toLocationId: raw.toLocationId!,
            reason: raw.reason,
          })
        : this.movementsApi.record(kind as SimpleMovementKind, {
            productId: raw.productId,
            gradeId: raw.gradeId,
            qty: raw.qty,
            reason: raw.reason,
            locationId: raw.locationId,
          });

    obs.subscribe({
      next: (movements) => {
        this.submitting.set(false);
        this.lastResult.set(movements);
        this.resetForm();
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        // A distinct, calm path for "not enough stock" (409 Insufficient stock) - a real, expected
        // user error (trying to waste/transfer more than is on hand), not a bug (task brief).
        this.insufficientStock.set(
          err.status === 409 &&
            (err.error as { title?: string } | null)?.title === 'Insufficient stock',
        );
        this.submitError.set(extractErrorMessage(err));
      },
    });
  }

  productName(id: number): string {
    const product = this.products().find((p) => p.productId === id);
    if (!product) return `#${id}`;
    return product.isActive ? product.name : `${product.name} (inactive)`;
  }

  gradeName(id: number | null): string {
    if (id === null) return '-';
    const grade = this.grades().find((g) => g.gradeId === id);
    if (!grade) return `#${id}`;
    return grade.isActive ? grade.name : `${grade.name} (inactive)`;
  }

  locationName(id: number | null): string {
    if (id === null) return '-';
    const location = this.locations().find((l) => l.locationId === id);
    if (!location) return `#${id}`;
    return location.isActive ? location.name : `${location.name} (inactive)`;
  }
}
