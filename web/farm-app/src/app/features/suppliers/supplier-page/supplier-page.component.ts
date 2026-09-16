import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CreateSupplierRequest, SupplierDto, UpdateSupplierRequest } from '../supplier.model';
import { SuppliersApiService } from '../suppliers-api.service';

@Component({
  selector: 'app-supplier-page',
  imports: [ReactiveFormsModule],
  templateUrl: './supplier-page.component.html',
  styleUrl: './supplier-page.component.scss',
})
export class SupplierPageComponent implements OnInit {
  private api = inject(SuppliersApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  suppliers = signal<SupplierDto[]>([]);
  showInactive = signal(false);

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    phone: this.fb.control<string | null>(null, [Validators.maxLength(50)]),
    notes: this.fb.control<string | null>(null, [Validators.maxLength(1000)]),
    vatNumber: this.fb.control<string | null>(null, [Validators.maxLength(20)]),
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    phone: this.fb.control<string | null>(null, [Validators.maxLength(50)]),
    notes: this.fb.control<string | null>(null, [Validators.maxLength(1000)]),
    vatNumber: this.fb.control<string | null>(null, [Validators.maxLength(20)]),
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.suppliers.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateSupplierRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ name: '', phone: null, notes: null, vatNumber: null });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(supplier: SupplierDto): void {
    this.editingId.set(supplier.supplierId);
    this.editError.set('');
    this.editForm.setValue({
      name: supplier.name,
      phone: supplier.phone,
      notes: supplier.notes,
      vatNumber: supplier.vatNumber,
      isActive: supplier.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateSupplierRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(supplier: SupplierDto): void {
    if (!confirm(`Deactivate supplier "${supplier.name}"?`)) return;
    this.api.deactivate(supplier.supplierId).subscribe(() => this.load());
  }
}
