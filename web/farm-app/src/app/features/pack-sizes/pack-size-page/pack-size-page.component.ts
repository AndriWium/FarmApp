import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { CreatePackSizeRequest, PackSizeDto, UpdatePackSizeRequest } from '../pack-size.model';
import { PackSizesApiService } from '../pack-sizes-api.service';

@Component({
  selector: 'app-pack-size-page',
  imports: [ReactiveFormsModule],
  templateUrl: './pack-size-page.component.html',
  styleUrl: './pack-size-page.component.scss',
})
export class PackSizePageComponent implements OnInit {
  private api = inject(PackSizesApiService);
  private productsApi = inject(ProductsApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  packSizes = signal<PackSizeDto[]>([]);

  // Includes inactive products too, so a pack size under a since-deactivated product still
  // resolves a name in the table (same reasoning as CultivarPageComponent/crops).
  products = signal<ProductDto[]>([]);
  productsById = computed(() => new Map(this.products().map((p) => [p.productId, p])));

  createForm = this.fb.nonNullable.group({
    productId: [0, [Validators.required, Validators.min(1)]],
    name: ['', [Validators.required, Validators.maxLength(50)]],
    qtyInBaseUnit: [0, [Validators.required, Validators.min(0.001)]],
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    productId: [0, [Validators.required, Validators.min(1)]],
    name: ['', [Validators.required, Validators.maxLength(50)]],
    qtyInBaseUnit: [0, [Validators.required, Validators.min(0.001)]],
  });
  editError = signal('');

  ngOnInit(): void {
    this.loadProducts();
    this.load();
  }

  loadProducts(): void {
    this.productsApi.getAll(true).subscribe((rows) => this.products.set(rows));
  }

  load(): void {
    this.api.getAll().subscribe((rows) => this.packSizes.set(rows));
  }

  productName(productId: number): string {
    const product = this.productsById().get(productId);
    if (!product) return `#${productId}`;
    return product.isActive ? product.name : `${product.name} (inactive)`;
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreatePackSizeRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ productId: 0, name: '', qtyInBaseUnit: 0 });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(packSize: PackSizeDto): void {
    this.editingId.set(packSize.packSizeId);
    this.editError.set('');
    this.editForm.setValue({
      productId: packSize.productId,
      name: packSize.name,
      qtyInBaseUnit: packSize.qtyInBaseUnit,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdatePackSizeRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  // No deactivate/reactivate here - PackSize has no IsActive column (model comment); this is a
  // real, permanent delete of the row.
  delete(packSize: PackSizeDto): void {
    if (!confirm(`Delete pack size "${packSize.name}"? This cannot be undone.`)) return;
    this.api.delete(packSize.packSizeId).subscribe(() => this.load());
  }
}
