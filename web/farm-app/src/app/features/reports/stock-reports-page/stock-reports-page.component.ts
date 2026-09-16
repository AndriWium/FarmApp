import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { currentMonthRange } from '../../../shared/date-range.util';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { StockMovementSummaryDto, StockOnHandDto } from '../report.model';
import { ReportsApiService } from '../reports-api.service';

/**
 * Stock report (doc 04 §3): on-hand valuation and the opening/in/out/closing movement summary for
 * a date range, both optionally narrowed by product/grade. The two sections share one page
 * (rather than two routes) since they're read together in practice - "what do I have" and "how did
 * it get to that" for the same product.
 *
 * The movement summary balances by construction (ReportQueries.GetStockMovementSummaryAsync's own
 * comment: Opening/In/Out/Closing are all partitions of the same SUM(Qty)) - this page still shows
 * the arithmetic explicitly (doc 04 §3: "if this doesn't balance, the report must say so loudly"),
 * verified per-row rather than trusted blindly.
 */
@Component({
  selector: 'app-stock-reports-page',
  imports: [DecimalPipe, FormsModule, RouterLink],
  templateUrl: './stock-reports-page.component.html',
  styleUrl: './stock-reports-page.component.scss',
})
export class StockReportsPageComponent implements OnInit {
  private api = inject(ReportsApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  private route = inject(ActivatedRoute);

  private defaultRange = currentMonthRange();
  from = signal(this.defaultRange.from);
  to = signal(this.defaultRange.to);
  productFilter = signal(0);
  gradeFilter = signal(0);

  products = signal<ProductDto[]>([]);
  productsById = computed(() => new Map(this.products().map((p) => [p.productId, p])));
  grades = signal<GradeDto[]>([]);

  loading = signal(true);
  error = signal('');

  onHandRows = signal<StockOnHandDto[]>([]);
  movementRows = signal<StockMovementSummaryDto[]>([]);

  filteredOnHandRows = computed(() => {
    const filter = this.productFilter();
    const rows = this.onHandRows();
    return filter === 0 ? rows : rows.filter((r) => r.productId === filter);
  });

  onHandTotalValue = computed(() => this.filteredOnHandRows().reduce((sum, r) => sum + r.value, 0));

  unbalancedMovementRows = computed(() =>
    this.movementRows().filter(
      (r) => Math.round((r.openingBalance + r.totalIn - r.totalOut - r.closingBalance) * 100) !== 0,
    ),
  );

  ngOnInit(): void {
    const productIdParam = this.route.snapshot.queryParamMap.get('productId');
    if (productIdParam) this.productFilter.set(Number(productIdParam));

    forkJoin({
      products: this.productsApi.getAll(true),
      grades: this.gradesApi.getAll(true),
    }).subscribe(({ products, grades }) => {
      this.products.set(products);
      this.grades.set(grades);
    });

    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    forkJoin({
      onHand: this.api.getStockOnHand(),
      movement: this.api.getStockMovementSummary(
        this.from(),
        this.to(),
        this.productFilter() || null,
        this.gradeFilter() || null,
      ),
    }).subscribe({
      next: ({ onHand, movement }) => {
        this.onHandRows.set(onHand);
        this.movementRows.set(movement);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load stock reports. Please try again.');
        this.loading.set(false);
      },
    });
  }

  productName(productId: number): string {
    const product = this.productsById().get(productId);
    if (!product) return `#${productId}`;
    return product.isActive ? product.name : `${product.name} (inactive)`;
  }
}
