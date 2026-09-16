import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { PriceListDto } from '../../price-lists/price-list.model';
import { PriceListsApiService } from '../../price-lists/price-lists-api.service';
import {
  CUSTOMER_TYPES,
  CreateCustomerRequest,
  CustomerDto,
  CustomerType,
  UpdateCustomerRequest,
} from '../customer.model';
import { CustomersApiService } from '../customers-api.service';

@Component({
  selector: 'app-customer-page',
  imports: [ReactiveFormsModule],
  templateUrl: './customer-page.component.html',
  styleUrl: './customer-page.component.scss',
})
export class CustomerPageComponent implements OnInit {
  private api = inject(CustomersApiService);
  private priceListsApi = inject(PriceListsApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  customerTypes = CUSTOMER_TYPES;
  canManage = computed(() => this.auth.hasRole('Owner'));

  customers = signal<CustomerDto[]>([]);
  showInactive = signal(false);

  // Includes inactive price lists too, so a customer pointing at a since-deactivated price list
  // still resolves a name in the table (same reasoning as CultivarPageComponent/crops).
  priceLists = signal<PriceListDto[]>([]);
  priceListsById = computed(() => new Map(this.priceLists().map((p) => [p.priceListId, p])));

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    phone: this.fb.control<string | null>(null, [Validators.maxLength(50)]),
    type: ['Retail' as CustomerType, [Validators.required]],
    priceListId: [0, [Validators.required, Validators.min(1)]],
    creditLimit: this.fb.control<number | null>(null, [Validators.min(0)]),
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    phone: this.fb.control<string | null>(null, [Validators.maxLength(50)]),
    type: ['Retail' as CustomerType, [Validators.required]],
    priceListId: [0, [Validators.required, Validators.min(1)]],
    creditLimit: this.fb.control<number | null>(null, [Validators.min(0)]),
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.loadPriceLists();
    this.load();
  }

  loadPriceLists(): void {
    this.priceListsApi.getAll(true).subscribe((rows) => this.priceLists.set(rows));
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.customers.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  priceListName(priceListId: number): string {
    const priceList = this.priceListsById().get(priceListId);
    if (!priceList) return `#${priceListId}`;
    return priceList.isActive ? priceList.name : `${priceList.name} (inactive)`;
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateCustomerRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({
          name: '',
          phone: null,
          type: 'Retail',
          priceListId: 0,
          creditLimit: null,
        });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(customer: CustomerDto): void {
    this.editingId.set(customer.customerId);
    this.editError.set('');
    this.editForm.setValue({
      name: customer.name,
      phone: customer.phone,
      type: customer.type,
      priceListId: customer.priceListId,
      creditLimit: customer.creditLimit,
      isActive: customer.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateCustomerRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(customer: CustomerDto): void {
    if (!confirm(`Deactivate customer "${customer.name}"?`)) return;
    this.api.deactivate(customer.customerId).subscribe(() => this.load());
  }
}
