import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CreatePriceListRequest, PriceListDto, UpdatePriceListRequest } from '../price-list.model';
import { PriceListsApiService } from '../price-lists-api.service';

@Component({
  selector: 'app-price-list-page',
  imports: [ReactiveFormsModule],
  templateUrl: './price-list-page.component.html',
  styleUrl: './price-list-page.component.scss',
})
export class PriceListPageComponent implements OnInit {
  private api = inject(PriceListsApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  priceLists = signal<PriceListDto[]>([]);
  showInactive = signal(false);

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.priceLists.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreatePriceListRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ name: '' });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(priceList: PriceListDto): void {
    this.editingId.set(priceList.priceListId);
    this.editError.set('');
    this.editForm.setValue({
      name: priceList.name,
      isActive: priceList.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdatePriceListRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(priceList: PriceListDto): void {
    if (!confirm(`Deactivate price list "${priceList.name}"?`)) return;
    this.api.deactivate(priceList.priceListId).subscribe(() => this.load());
  }
}
