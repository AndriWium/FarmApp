import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { InputItemDto } from '../../input-items/input-item.model';
import { InputItemsApiService } from '../../input-items/input-items-api.service';
import { SupplierDto } from '../../suppliers/supplier.model';
import { SuppliersApiService } from '../../suppliers/suppliers-api.service';
import { InputPurchaseDto } from '../input-purchase.model';
import { InputPurchasesApiService } from '../input-purchases-api.service';

@Component({
  selector: 'app-input-purchase-detail-page',
  imports: [DatePipe, DecimalPipe, RouterLink],
  templateUrl: './input-purchase-detail-page.component.html',
  styleUrl: './input-purchase-detail-page.component.scss',
})
export class InputPurchaseDetailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private api = inject(InputPurchasesApiService);
  private suppliersApi = inject(SuppliersApiService);
  private inputItemsApi = inject(InputItemsApiService);

  loading = signal(true);
  notFound = signal(false);
  purchase = signal<InputPurchaseDto | null>(null);
  suppliers = signal<SupplierDto[]>([]);
  inputItems = signal<InputItemDto[]>([]);
  // InputItemId -> current on-hand (InputPurchaseLineDto carries none itself - see
  // InputItemsApiService.getOnHand) so a farmer reviewing an old purchase can see what's left.
  onHandByItemId = signal<Map<number, number>>(new Map());

  // Subscribes to paramMap rather than reading route.snapshot once - Angular's default route-
  // reuse strategy keeps this component instance alive when navigating directly between two
  // /input-purchases/:id routes, same fix ProductRecipePageComponent needed (DECISIONS.md Phase 5c-1).
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
      inputItems: this.inputItemsApi.getAll(true),
    }).subscribe({
      next: ({ purchase, suppliers, inputItems }) => {
        this.purchase.set(purchase);
        this.suppliers.set(suppliers);
        this.inputItems.set(inputItems);
        this.loading.set(false);
        this.loadOnHand(purchase);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  private loadOnHand(purchase: InputPurchaseDto): void {
    const itemIds = [...new Set(purchase.lines.map((l) => l.inputItemId))];
    if (itemIds.length === 0) return;
    forkJoin(itemIds.map((id) => this.inputItemsApi.getOnHand(id))).subscribe((onHands) => {
      this.onHandByItemId.set(new Map(itemIds.map((id, i) => [id, onHands[i]])));
    });
  }

  supplierName(id: number): string {
    const supplier = this.suppliers().find((s) => s.supplierId === id);
    if (!supplier) return `#${id}`;
    return supplier.isActive ? supplier.name : `${supplier.name} (inactive)`;
  }

  inputItemName(id: number): string {
    const item = this.inputItems().find((i) => i.inputItemId === id);
    if (!item) return `#${id}`;
    return item.isActive ? item.name : `${item.name} (inactive)`;
  }

  inputItemUnit(id: number): string {
    const item = this.inputItems().find((i) => i.inputItemId === id);
    return item ? item.unit : '';
  }

  totalCost(): number {
    const purchase = this.purchase();
    if (!purchase) return 0;
    return purchase.lines.reduce((sum, l) => sum + l.qty * l.unitCost, 0);
  }
}
