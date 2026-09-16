import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { inject } from '@angular/core';
import { forkJoin } from 'rxjs';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { SupplierDto } from '../../suppliers/supplier.model';
import { SuppliersApiService } from '../../suppliers/suppliers-api.service';
import { ProducePurchaseDto } from '../produce-purchase.model';
import { ProducePurchasesApiService } from '../produce-purchases-api.service';

@Component({
  selector: 'app-produce-purchase-detail-page',
  imports: [DatePipe, DecimalPipe, RouterLink],
  templateUrl: './produce-purchase-detail-page.component.html',
  styleUrl: './produce-purchase-detail-page.component.scss',
})
export class ProducePurchaseDetailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private api = inject(ProducePurchasesApiService);
  private suppliersApi = inject(SuppliersApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);

  loading = signal(true);
  notFound = signal(false);
  purchase = signal<ProducePurchaseDto | null>(null);
  suppliers = signal<SupplierDto[]>([]);
  products = signal<ProductDto[]>([]);
  grades = signal<GradeDto[]>([]);

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
      purchase: this.api.getById(id),
      suppliers: this.suppliersApi.getAll(true),
      products: this.productsApi.getAll(true),
      grades: this.gradesApi.getAll(true),
    }).subscribe({
      next: ({ purchase, suppliers, products, grades }) => {
        this.purchase.set(purchase);
        this.suppliers.set(suppliers);
        this.products.set(products);
        this.grades.set(grades);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  supplierName(id: number): string {
    const supplier = this.suppliers().find((s) => s.supplierId === id);
    if (!supplier) return `#${id}`;
    return supplier.isActive ? supplier.name : `${supplier.name} (inactive)`;
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

  totalCost(): number {
    const purchase = this.purchase();
    if (!purchase) return 0;
    return purchase.lines.reduce((sum, l) => sum + l.qty * l.unitCost, 0);
  }
}
