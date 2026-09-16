import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CropDto } from '../../crops/crop.model';
import { CropsApiService } from '../../crops/crops-api.service';
import {
  CreateProductRequest,
  MAKE_MODES,
  MakeMode,
  PRODUCT_BASE_UNITS,
  PRODUCT_TYPES,
  ProductBaseUnit,
  ProductDto,
  ProductType,
  UpdateProductRequest,
} from '../product.model';
import { ProductsApiService } from '../products-api.service';

@Component({
  selector: 'app-product-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './product-page.component.html',
  styleUrl: './product-page.component.scss',
})
export class ProductPageComponent implements OnInit {
  private api = inject(ProductsApiService);
  private cropsApi = inject(CropsApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  productTypes = PRODUCT_TYPES;
  makeModes = MAKE_MODES;
  baseUnits = PRODUCT_BASE_UNITS;

  canManage = computed(() => this.auth.hasRole('Owner'));

  products = signal<ProductDto[]>([]);
  showInactive = signal(false);

  // Includes inactive crops too, so a product whose crop was since deactivated still resolves a
  // name in the table (same reasoning as CultivarPageComponent).
  crops = signal<CropDto[]>([]);
  cropsById = computed(() => new Map(this.crops().map((c) => [c.cropId, c])));

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    productType: ['Produce' as ProductType, [Validators.required]],
    cropId: this.fb.control<number | null>(null),
    makeMode: this.fb.control<MakeMode | null>(null),
    baseUnit: ['Kg' as ProductBaseUnit, [Validators.required]],
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    productType: ['Produce' as ProductType, [Validators.required]],
    cropId: this.fb.control<number | null>(null),
    makeMode: this.fb.control<MakeMode | null>(null),
    baseUnit: ['Kg' as ProductBaseUnit, [Validators.required]],
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.loadCrops();
    this.load();

    // Conditional-field logic (task brief): CropId only meaningful for Produce, MakeMode only for
    // Prepared. Rather than always showing every field regardless of type, the crop/make-mode
    // controls are enabled+required only when relevant, and cleared+disabled otherwise - a
    // disabled control's stale value is excluded from Angular's validity check but still present
    // via getRawValue(), so it's explicitly reset to null here to avoid submitting a leftover
    // value from a previously-selected type.
    this.createForm.controls.productType.valueChanges.subscribe((type) =>
      this.syncConditionalControls(this.createForm, type),
    );
    this.editForm.controls.productType.valueChanges.subscribe((type) =>
      this.syncConditionalControls(this.editForm, type),
    );
    this.syncConditionalControls(this.createForm, this.createForm.controls.productType.value);
  }

  private syncConditionalControls(
    form: typeof this.createForm | typeof this.editForm,
    type: ProductType,
  ): void {
    if (type === 'Produce') {
      form.controls.cropId.enable({ emitEvent: false });
      form.controls.cropId.setValidators([Validators.required, Validators.min(1)]);
    } else {
      form.controls.cropId.setValue(null, { emitEvent: false });
      form.controls.cropId.clearValidators();
      form.controls.cropId.disable({ emitEvent: false });
    }
    form.controls.cropId.updateValueAndValidity({ emitEvent: false });

    if (type === 'Prepared') {
      form.controls.makeMode.enable({ emitEvent: false });
      form.controls.makeMode.setValidators([Validators.required]);
    } else {
      form.controls.makeMode.setValue(null, { emitEvent: false });
      form.controls.makeMode.clearValidators();
      form.controls.makeMode.disable({ emitEvent: false });
    }
    form.controls.makeMode.updateValueAndValidity({ emitEvent: false });
  }

  loadCrops(): void {
    this.cropsApi.getAll(true).subscribe((rows) => this.crops.set(rows));
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.products.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  cropName(cropId: number | null): string {
    if (cropId === null) return '-';
    const crop = this.cropsById().get(cropId);
    if (!crop) return `#${cropId}`;
    return crop.isActive ? crop.name : `${crop.name} (inactive)`;
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateProductRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({
          name: '',
          productType: 'Produce',
          cropId: null,
          makeMode: null,
          baseUnit: 'Kg',
        });
        this.syncConditionalControls(this.createForm, 'Produce');
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(product: ProductDto): void {
    this.editingId.set(product.productId);
    this.editError.set('');
    this.syncConditionalControls(this.editForm, product.productType);
    this.editForm.setValue({
      name: product.name,
      productType: product.productType,
      cropId: product.cropId,
      makeMode: product.makeMode,
      baseUnit: product.baseUnit,
      isActive: product.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateProductRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(product: ProductDto): void {
    if (!confirm(`Deactivate product "${product.name}"?`)) return;
    this.api.deactivate(product.productId).subscribe(() => this.load());
  }
}
