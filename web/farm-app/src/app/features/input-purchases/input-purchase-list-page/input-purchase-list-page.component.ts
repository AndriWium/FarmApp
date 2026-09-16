import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { InputItemDto } from '../../input-items/input-item.model';
import { InputItemsApiService } from '../../input-items/input-items-api.service';
import { SupplierDto } from '../../suppliers/supplier.model';
import { SuppliersApiService } from '../../suppliers/suppliers-api.service';
import { InputPurchaseDto } from '../input-purchase.model';
import { InputPurchasesApiService } from '../input-purchases-api.service';

@Component({
  selector: 'app-input-purchase-list-page',
  imports: [DatePipe, DecimalPipe, RouterLink],
  templateUrl: './input-purchase-list-page.component.html',
  styleUrl: './input-purchase-list-page.component.scss',
})
export class InputPurchaseListPageComponent implements OnInit {
  private api = inject(InputPurchasesApiService);
  private suppliersApi = inject(SuppliersApiService);
  private inputItemsApi = inject(InputItemsApiService);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  loading = signal(true);
  purchases = signal<InputPurchaseDto[]>([]);

  // Includes inactive rows too - same "still resolves, not still selectable" convention as every
  // other cross-entity lookup in this app (see CultivarPage).
  suppliers = signal<SupplierDto[]>([]);
  suppliersById = computed(() => new Map(this.suppliers().map((s) => [s.supplierId, s])));
  inputItems = signal<InputItemDto[]>([]);
  inputItemsById = computed(() => new Map(this.inputItems().map((i) => [i.inputItemId, i])));

  sortedPurchases = computed(() =>
    [...this.purchases()].sort((a, b) => b.date.localeCompare(a.date) || b.inputPurchaseId - a.inputPurchaseId),
  );

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      purchases: this.api.getAll(),
      suppliers: this.suppliersApi.getAll(true),
      inputItems: this.inputItemsApi.getAll(true),
    }).subscribe(({ purchases, suppliers, inputItems }) => {
      this.purchases.set(purchases);
      this.suppliers.set(suppliers);
      this.inputItems.set(inputItems);
      this.loading.set(false);
    });
  }

  supplierName(id: number): string {
    const supplier = this.suppliersById().get(id);
    if (!supplier) return `#${id}`;
    return supplier.isActive ? supplier.name : `${supplier.name} (inactive)`;
  }

  inputItemName(id: number): string {
    const item = this.inputItemsById().get(id);
    if (!item) return `#${id}`;
    return item.isActive ? item.name : `${item.name} (inactive)`;
  }

  lineTotal(purchase: InputPurchaseDto): number {
    return purchase.lines.reduce((sum, l) => sum + l.qty * l.unitCost, 0);
  }
}
