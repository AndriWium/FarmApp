import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CreateCropRequest, CropDto, UpdateCropRequest } from '../crop.model';
import { CropsApiService } from '../crops-api.service';

@Component({
  selector: 'app-crop-page',
  imports: [ReactiveFormsModule],
  templateUrl: './crop-page.component.html',
  styleUrl: './crop-page.component.scss',
})
export class CropPageComponent implements OnInit {
  private api = inject(CropsApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  crops = signal<CropDto[]>([]);
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
    this.api.getAll(this.showInactive()).subscribe((rows) => this.crops.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateCropRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ name: '' });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(crop: CropDto): void {
    this.editingId.set(crop.cropId);
    this.editError.set('');
    this.editForm.setValue({ name: crop.name, isActive: crop.isActive });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateCropRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(crop: CropDto): void {
    if (!confirm(`Deactivate crop "${crop.name}"?`)) return;
    this.api.deactivate(crop.cropId).subscribe(() => this.load());
  }
}
