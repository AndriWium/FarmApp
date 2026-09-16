import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { forkJoin, map, of, switchMap } from 'rxjs';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { StockBatchesApiService } from '../stock-batches-api.service';
import { StockBatchDto } from '../stock-batch.model';

interface StockBatchRow extends StockBatchDto {
  onHand: number;
}

@Component({
  selector: 'app-stock-on-hand-page',
  imports: [DatePipe, DecimalPipe, FormsModule],
  templateUrl: './stock-on-hand-page.component.html',
  styleUrl: './stock-on-hand-page.component.scss',
})
export class StockOnHandPageComponent implements OnInit {
  private batchesApi = inject(StockBatchesApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  private route = inject(ActivatedRoute);

  loading = signal(true);
  rows = signal<StockBatchRow[]>([]);

  // Includes inactive products/grades so a batch of a since-deactivated product still resolves a
  // name (same convention as every other cross-entity lookup in this app - see CultivarPage).
  products = signal<ProductDto[]>([]);
  productsById = computed(() => new Map(this.products().map((p) => [p.productId, p])));
  grades = signal<GradeDto[]>([]);
  gradesById = computed(() => new Map(this.grades().map((g) => [g.gradeId, g])));

  productFilter = signal(0);

  filteredRows = computed(() => {
    const filter = this.productFilter();
    const rows = this.rows();
    return filter === 0 ? rows : rows.filter((r) => r.productId === filter);
  });

  // Zero-on-hand batches (fully depleted/wasted) are hidden by default - a farmer checking "what
  // do I actually have" doesn't want a long tail of empty historical batches cluttering the view.
  showDepleted = signal(false);

  visibleRows = computed(() => {
    const rows = this.filteredRows();
    return this.showDepleted() ? rows : rows.filter((r) => r.onHand > 0);
  });

  ngOnInit(): void {
    // Supports deep-linking from the Stock Reports report page (doc 04's drillability principle) -
    // a ?productId= query param preselects the same client-side filter the dropdown itself sets.
    const productIdParam = this.route.snapshot.queryParamMap.get('productId');
    if (productIdParam) this.productFilter.set(Number(productIdParam));

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

  toggleShowDepleted(): void {
    this.showDepleted.update((v) => !v);
  }
}
