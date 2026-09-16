import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { SupplierDto } from '../../suppliers/supplier.model';
import { SuppliersApiService } from '../../suppliers/suppliers-api.service';
import { ProducePurchaseDto } from '../produce-purchase.model';
import { ProducePurchasesApiService } from '../produce-purchases-api.service';

@Component({
  selector: 'app-produce-purchase-list-page',
  imports: [DatePipe, DecimalPipe, RouterLink],
  templateUrl: './produce-purchase-list-page.component.html',
  styleUrl: './produce-purchase-list-page.component.scss',
})
export class ProducePurchaseListPageComponent implements OnInit {
  private api = inject(ProducePurchasesApiService);
  private suppliersApi = inject(SuppliersApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  loading = signal(true);
  purchases = signal<ProducePurchaseDto[]>([]);

  // Includes inactive rows too - same "still resolves, not still selectable" convention as every
  // other cross-entity lookup in this app (see CultivarPage).
  suppliers = signal<SupplierDto[]>([]);
  suppliersById = computed(() => new Map(this.suppliers().map((s) => [s.supplierId, s])));
  products = signal<ProductDto[]>([]);
  productsById = computed(() => new Map(this.products().map((p) => [p.productId, p])));
  grades = signal<GradeDto[]>([]);
  gradesById = computed(() => new Map(this.grades().map((g) => [g.gradeId, g])));

  sortedPurchases = computed(() =>
    [...this.purchases()].sort((a, b) => b.date.localeCompare(a.date) || b.producePurchaseId - a.producePurchaseId),
  );

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      purchases: this.api.getAll(),
      suppliers: this.suppliersApi.getAll(true),
      products: this.productsApi.getAll(true),
      grades: this.gradesApi.getAll(true),
    }).subscribe(({ purchases, suppliers, products, grades }) => {
      this.purchases.set(purchases);
      this.suppliers.set(suppliers);
      this.products.set(products);
      this.grades.set(grades);
      this.loading.set(false);
    });
  }

  supplierName(id: number): string {
    const supplier = this.suppliersById().get(id);
    if (!supplier) return `#${id}`;
    return supplier.isActive ? supplier.name : `${supplier.name} (inactive)`;
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

  lineTotal(purchase: ProducePurchaseDto): number {
    return purchase.lines.reduce((sum, l) => sum + l.qty * l.unitCost, 0);
  }
}
