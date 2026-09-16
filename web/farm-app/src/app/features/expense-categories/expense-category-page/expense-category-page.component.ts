import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import {
  CreateExpenseCategoryRequest,
  ExpenseCategoryDto,
  UpdateExpenseCategoryRequest,
} from '../expense-category.model';
import { ExpenseCategoriesApiService } from '../expense-categories-api.service';

@Component({
  selector: 'app-expense-category-page',
  imports: [ReactiveFormsModule],
  templateUrl: './expense-category-page.component.html',
  styleUrl: './expense-category-page.component.scss',
})
export class ExpenseCategoryPageComponent implements OnInit {
  private api = inject(ExpenseCategoriesApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  categories = signal<ExpenseCategoryDto[]>([]);
  showInactive = signal(false);

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    isFarmingDirect: [false],
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    isFarmingDirect: [false],
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.categories.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateExpenseCategoryRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ name: '', isFarmingDirect: false });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(category: ExpenseCategoryDto): void {
    this.editingId.set(category.expenseCategoryId);
    this.editError.set('');
    this.editForm.setValue({
      name: category.name,
      isFarmingDirect: category.isFarmingDirect,
      isActive: category.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateExpenseCategoryRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(category: ExpenseCategoryDto): void {
    if (!confirm(`Deactivate expense category "${category.name}"?`)) return;
    this.api.deactivate(category.expenseCategoryId).subscribe(() => this.load());
  }
}
