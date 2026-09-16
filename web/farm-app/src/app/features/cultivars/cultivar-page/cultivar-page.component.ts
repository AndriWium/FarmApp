import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CropDto } from '../../crops/crop.model';
import { CropsApiService } from '../../crops/crops-api.service';
import { CreateCultivarRequest, CultivarDto, UpdateCultivarRequest } from '../cultivar.model';
import { CultivarsApiService } from '../cultivars-api.service';

@Component({
  selector: 'app-cultivar-page',
  imports: [ReactiveFormsModule],
  templateUrl: './cultivar-page.component.html',
  styleUrl: './cultivar-page.component.scss',
})
export class CultivarPageComponent implements OnInit {
  private api = inject(CultivarsApiService);
  private cropsApi = inject(CropsApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  cultivars = signal<CultivarDto[]>([]);
  showInactive = signal(false);

  // Includes inactive crops too, so a cultivar whose crop was since deactivated still resolves a
  // name in the table and remains a valid (if visually marked) choice when editing that cultivar.
  crops = signal<CropDto[]>([]);
  cropsById = computed(() => new Map(this.crops().map((c) => [c.cropId, c])));

  createForm = this.fb.nonNullable.group({
    cropId: [0, [Validators.required, Validators.min(1)]],
    name: ['', [Validators.required, Validators.maxLength(50)]],
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    cropId: [0, [Validators.required, Validators.min(1)]],
    name: ['', [Validators.required, Validators.maxLength(50)]],
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.loadCrops();
    this.load();
  }

  loadCrops(): void {
    this.cropsApi.getAll(true).subscribe((rows) => this.crops.set(rows));
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.cultivars.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  cropName(cropId: number): string {
    const crop = this.cropsById().get(cropId);
    if (!crop) return `#${cropId}`;
    return crop.isActive ? crop.name : `${crop.name} (inactive)`;
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateCultivarRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ cropId: 0, name: '' });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(cultivar: CultivarDto): void {
    this.editingId.set(cultivar.cultivarId);
    this.editError.set('');
    this.editForm.setValue({
      cropId: cultivar.cropId,
      name: cultivar.name,
      isActive: cultivar.isActive,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateCultivarRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(cultivar: CultivarDto): void {
    if (!confirm(`Deactivate cultivar "${cultivar.name}"?`)) return;
    this.api.deactivate(cultivar.cultivarId).subscribe(() => this.load());
  }
}
