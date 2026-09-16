import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import {
  CreateInputItemRequest,
  INPUT_ITEM_CATEGORIES,
  InputItemDto,
  UpdateInputItemRequest,
} from '../input-item.model';
import { InputItemsApiService } from '../input-items-api.service';

@Component({
  selector: 'app-input-item-page',
  imports: [ReactiveFormsModule],
  templateUrl: './input-item-page.component.html',
  styleUrl: './input-item-page.component.scss',
})
export class InputItemPageComponent implements OnInit {
  private api = inject(InputItemsApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  categories = INPUT_ITEM_CATEGORIES;
  canManage = computed(() => this.auth.hasRole('Owner'));

  items = signal<InputItemDto[]>([]);
  showInactive = signal(false);

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    category: ['Other' as InputItemDto['category'], [Validators.required]],
    unit: ['', [Validators.required, Validators.maxLength(20)]],
    reorderLevel: [0, [Validators.required, Validators.min(0)]],
    withholdingDays: this.fb.control<number | null>(null, [Validators.min(0)]),
    activeIngredient: this.fb.control<string | null>(null, [Validators.maxLength(100)]),
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    category: ['Other' as InputItemDto['category'], [Validators.required]],
    unit: ['', [Validators.required, Validators.maxLength(20)]],
    reorderLevel: [0, [Validators.required, Validators.min(0)]],
    withholdingDays: this.fb.control<number | null>(null, [Validators.min(0)]),
    activeIngredient: this.fb.control<string | null>(null, [Validators.maxLength(100)]),
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.items.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateInputItemRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({
          name: '',
          category: 'Other',
          unit: '',
          reorderLevel: 0,
          withholdingDays: null,
          activeIngredient: null,
        });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(item: InputItemDto): void {
    this.editingId.set(item.inputItemId);
    this.editError.set('');
    this.editForm.setValue({
      name: item.name,
      category: item.category,
      unit: item.unit,
      reorderLevel: item.reorderLevel,
      withholdingDays: item.withholdingDays,
      activeIngredient: item.activeIngredient,
      isActive: item.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateInputItemRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(item: InputItemDto): void {
    if (!confirm(`Deactivate input item "${item.name}"?`)) return;
    this.api.deactivate(item.inputItemId).subscribe(() => this.load());
  }
}
