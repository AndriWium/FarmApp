import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { SupplierDto } from '../../suppliers/supplier.model';
import { SuppliersApiService } from '../../suppliers/suppliers-api.service';
import { CreatePurchaseLineRequest, ProducePurchaseDto } from '../produce-purchase.model';
import { ProducePurchasesApiService } from '../produce-purchases-api.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

@Component({
  selector: 'app-produce-purchase-form-page',
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe],
  templateUrl: './produce-purchase-form-page.component.html',
  styleUrl: './produce-purchase-form-page.component.scss',
})
export class ProducePurchaseFormPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(ProducePurchasesApiService);
  private suppliersApi = inject(SuppliersApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  suppliers = signal<SupplierDto[]>([]);
  products = signal<ProductDto[]>([]);
  grades = signal<GradeDto[]>([]);

  header = this.fb.nonNullable.group({
    supplierId: [0, [Validators.required, Validators.min(1)]],
    date: [todayIso(), [Validators.required]],
    invoiceRef: this.fb.control<string | null>(null, [Validators.maxLength(50)]),
  });

  lines = this.fb.array<ReturnType<typeof this.newLineGroup>>([]);

  submitting = signal(false);
  submitError = signal('');
  created = signal<ProducePurchaseDto | null>(null);

  ngOnInit(): void {
    this.suppliersApi.getAll().subscribe((rows) => this.suppliers.set(rows));
    this.productsApi.getAll().subscribe((rows) => this.products.set(rows));
    this.gradesApi.getAll().subscribe((rows) => this.grades.set(rows));
    this.addLine();
  }

  private newLineGroup() {
    return this.fb.nonNullable.group({
      productId: [0, [Validators.required, Validators.min(1)]],
      gradeId: this.fb.control<number | null>(null),
      qty: [0, [Validators.required, Validators.min(0.001)]],
      unitCost: [0, [Validators.required, Validators.min(0)]],
      shelfLifeDays: [7, [Validators.required, Validators.min(1)]],
      vatAmount: this.fb.control<number | null>(null, [Validators.min(0)]),
    });
  }

  addLine(): void {
    this.lines.push(this.newLineGroup());
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  totalCost(): number {
    return this.lines.controls.reduce((sum, line) => {
      const raw = line.getRawValue();
      return sum + raw.qty * raw.unitCost;
    }, 0);
  }

  submit(): void {
    if (this.header.invalid || this.lines.invalid || this.lines.length === 0) {
      this.header.markAllAsTouched();
      this.lines.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set('');
    this.created.set(null);

    const headerRaw = this.header.getRawValue();
    const lineRequests: CreatePurchaseLineRequest[] = this.lines.controls.map((c) => c.getRawValue());

    this.api
      .create({
        supplierId: headerRaw.supplierId,
        date: headerRaw.date,
        invoiceRef: headerRaw.invoiceRef,
        lines: lineRequests,
      })
      .subscribe({
        next: (purchase) => {
          this.submitting.set(false);
          this.created.set(purchase);
          this.resetForm();
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          this.submitError.set(extractErrorMessage(err));
        },
      });
  }

  private resetForm(): void {
    this.header.reset({ supplierId: 0, date: todayIso(), invoiceRef: null });
    this.lines.clear();
    this.addLine();
  }

  productName(id: number): string {
    const product = this.products().find((p) => p.productId === id);
    return product ? product.name : `#${id}`;
  }

  gradeName(id: number | null): string {
    if (id === null) return '-';
    const grade = this.grades().find((g) => g.gradeId === id);
    return grade ? grade.name : `#${id}`;
  }
}
