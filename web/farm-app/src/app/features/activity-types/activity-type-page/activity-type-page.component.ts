import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import {
  ActivityTypeDto,
  CreateActivityTypeRequest,
  UpdateActivityTypeRequest,
} from '../activity-type.model';
import { ActivityTypesApiService } from '../activity-types-api.service';

@Component({
  selector: 'app-activity-type-page',
  imports: [ReactiveFormsModule],
  templateUrl: './activity-type-page.component.html',
  styleUrl: './activity-type-page.component.scss',
})
export class ActivityTypePageComponent implements OnInit {
  private api = inject(ActivityTypesApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  activityTypes = signal<ActivityTypeDto[]>([]);
  showInactive = signal(false);

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    category: ['', [Validators.required, Validators.maxLength(50)]],
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    category: ['', [Validators.required, Validators.maxLength(50)]],
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.activityTypes.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateActivityTypeRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ name: '', category: '' });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(activityType: ActivityTypeDto): void {
    this.editingId.set(activityType.activityTypeId);
    this.editError.set('');
    this.editForm.setValue({
      name: activityType.name,
      category: activityType.category,
      isActive: activityType.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateActivityTypeRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(activityType: ActivityTypeDto): void {
    if (!confirm(`Deactivate activity type "${activityType.name}"?`)) return;
    this.api.deactivate(activityType.activityTypeId).subscribe(() => this.load());
  }
}
