import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { InputItemDto } from '../../input-items/input-item.model';
import { InputItemsApiService } from '../../input-items/input-items-api.service';
import { ProductWithRecipeDto, RecipeLineRequest } from '../product.model';
import { ProductsApiService } from '../products-api.service';

@Component({
  selector: 'app-product-recipe-page',
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe],
  templateUrl: './product-recipe-page.component.html',
  styleUrl: './product-recipe-page.component.scss',
})
export class ProductRecipePageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private productsApi = inject(ProductsApiService);
  private inputItemsApi = inject(InputItemsApiService);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  productId = 0;
  loading = signal(true);
  notFound = signal(false);
  product = signal<ProductWithRecipeDto | null>(null);

  // Only active input items are offered for a NEW line - an inactive one already on the recipe
  // stays visible (it's a valid existing value) but isn't a choice for a freshly added row (same
  // "still resolves, not still selectable" convention as PackSize's ProductId dropdown).
  inputItems = signal<InputItemDto[]>([]);
  inputItemsById = computed(() => new Map(this.inputItems().map((i) => [i.inputItemId, i])));
  costByInputItemId = signal<Map<number, number>>(new Map());

  lines = this.fb.array<ReturnType<typeof this.newLineGroup>>([]);

  saving = signal(false);
  saveError = signal('');
  saved = signal(false);

  totalCost = computed(() => {
    const costs = this.costByInputItemId();
    return this.lines.controls.reduce((sum, line) => {
      const raw = line.getRawValue();
      const cost = costs.get(raw.inputItemId) ?? 0;
      return sum + cost * (raw.qty || 0);
    }, 0);
  });

  ngOnInit(): void {
    // Subscribe rather than read route.snapshot once - Angular's default route-reuse strategy
    // keeps this component instance alive when navigating between two :id values on the SAME
    // route config (e.g. /products/3/recipe -> /products/9/recipe), so a snapshot-only read would
    // silently keep showing the first product's recipe after such a navigation.
    this.route.paramMap.subscribe((params) => {
      const idParam = params.get('id');
      this.productId = idParam ? Number(idParam) : 0;
      if (!this.productId) {
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
    this.saved.set(false);
    this.saveError.set('');
    forkJoin({
      product: this.productsApi.getRecipe(this.productId),
      inputItems: this.inputItemsApi.getAll(true),
    }).subscribe({
      next: ({ product, inputItems }) => {
        this.product.set(product);
        this.inputItems.set(inputItems.filter((i) => i.isActive));

        this.lines.clear();
        for (const line of product.recipeLines) {
          this.lines.push(this.newLineGroup(line.inputItemId, line.qty));
        }

        this.loadCosts(product.recipeLines.map((l) => l.inputItemId));
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  private loadCosts(recipeItemIds: number[]): void {
    const ids = new Set([...this.inputItems().map((i) => i.inputItemId), ...recipeItemIds]);
    if (ids.size === 0) {
      this.costByInputItemId.set(new Map());
      return;
    }
    const idList = [...ids];
    forkJoin(
      idList.map((id) =>
        // A brand-new input item with no purchases yet has no cost to average (404) - treat it as
        // zero rather than letting one failed lookup break every other line's cost display.
        this.inputItemsApi.getWeightedAverageCost(id).pipe(catchError(() => of(0))),
      ),
    ).subscribe((costs) => {
      this.costByInputItemId.set(new Map(idList.map((id, i) => [id, costs[i]])));
    });
  }

  private newLineGroup(inputItemId: number | null = null, qty: number | null = null) {
    return this.fb.nonNullable.group({
      inputItemId: this.fb.nonNullable.control<number>(inputItemId ?? 0, [
        Validators.required,
        Validators.min(1),
      ]),
      qty: this.fb.nonNullable.control<number>(qty ?? 0, [Validators.required, Validators.min(0.001)]),
    });
  }

  addLine(): void {
    this.lines.push(this.newLineGroup());
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  inputItemName(id: number): string {
    const item = this.inputItemsById().get(id);
    return item ? item.name : `#${id}`;
  }

  costFor(id: number): number {
    return this.costByInputItemId().get(id) ?? 0;
  }

  save(): void {
    if (this.lines.invalid) {
      this.lines.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.saveError.set('');
    this.saved.set(false);

    const req: { lines: RecipeLineRequest[] } = {
      lines: this.lines.controls.map((c) => c.getRawValue()),
    };

    this.productsApi.setRecipe(this.productId, req).subscribe({
      next: (product) => {
        this.saving.set(false);
        this.saved.set(true);
        this.product.set(product);
        this.loadCosts(product.recipeLines.map((l) => l.inputItemId));
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.saveError.set(extractErrorMessage(err));
      },
    });
  }

  back(): void {
    this.router.navigateByUrl('/products');
  }
}
