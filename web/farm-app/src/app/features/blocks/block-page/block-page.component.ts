import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { BlockDto, CreateBlockRequest, UpdateBlockRequest } from '../block.model';
import { BlocksApiService } from '../blocks-api.service';

@Component({
  selector: 'app-block-page',
  imports: [ReactiveFormsModule],
  templateUrl: './block-page.component.html',
  styleUrl: './block-page.component.scss',
})
export class BlockPageComponent implements OnInit {
  private api = inject(BlocksApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  blocks = signal<BlockDto[]>([]);
  showInactive = signal(false);

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    areaHectare: [0, [Validators.required, Validators.min(0.0001)]],
    note: ['', [Validators.maxLength(500)]],
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    areaHectare: [0, [Validators.required, Validators.min(0.0001)]],
    note: ['', [Validators.maxLength(500)]],
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.blocks.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateBlockRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ name: '', areaHectare: 0, note: '' });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(block: BlockDto): void {
    this.editingId.set(block.blockId);
    this.editError.set('');
    this.editForm.setValue({
      name: block.name,
      areaHectare: block.areaHectare,
      note: block.note,
      isActive: block.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateBlockRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(block: BlockDto): void {
    if (!confirm(`Deactivate block "${block.name}"?`)) return;
    this.api.deactivate(block.blockId).subscribe(() => this.load());
  }
}
