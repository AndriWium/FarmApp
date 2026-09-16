import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { BlockDto } from '../../blocks/block.model';
import { BlocksApiService } from '../../blocks/blocks-api.service';
import { CropDto } from '../../crops/crop.model';
import { CropsApiService } from '../../crops/crops-api.service';
import { CultivarDto } from '../../cultivars/cultivar.model';
import { CultivarsApiService } from '../../cultivars/cultivars-api.service';
import {
  CreatePlantingRequest,
  PLANTING_TYPES,
  PlantingDto,
  PlantingType,
  UpdatePlantingRequest,
} from '../planting.model';
import { PlantingsApiService } from '../plantings-api.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

@Component({
  selector: 'app-planting-page',
  imports: [ReactiveFormsModule],
  templateUrl: './planting-page.component.html',
  styleUrl: './planting-page.component.scss',
})
export class PlantingPageComponent implements OnInit {
  private api = inject(PlantingsApiService);
  private blocksApi = inject(BlocksApiService);
  private cropsApi = inject(CropsApiService);
  private cultivarsApi = inject(CultivarsApiService);
  private fb = inject(FormBuilder);

  // No canManage gate anywhere on this page - PlantingsController carries no
  // CanManageMasterData attribute on any verb (verified directly against the controller; see
  // DECISIONS.md Phase 3a "day-to-day farm capture, same footing as TillSession/Sale"), unlike
  // every master-data screen (Cultivar, InputItem, ...) which gates create/edit to Owner.
  types = PLANTING_TYPES;

  plantings = signal<PlantingDto[]>([]);

  // Includes inactive blocks/cultivars too, so a planting against a since-deactivated block or
  // cultivar still resolves a name (same convention as every other cross-entity lookup - see
  // CultivarPage).
  blocks = signal<BlockDto[]>([]);
  blocksById = computed(() => new Map(this.blocks().map((b) => [b.blockId, b])));
  crops = signal<CropDto[]>([]);
  cropsById = computed(() => new Map(this.crops().map((c) => [c.cropId, c])));
  cultivars = signal<CultivarDto[]>([]);
  cultivarsById = computed(() => new Map(this.cultivars().map((c) => [c.cultivarId, c])));

  createForm = this.fb.nonNullable.group({
    blockId: [0, [Validators.required, Validators.min(1)]],
    cultivarId: [0, [Validators.required, Validators.min(1)]],
    startDate: [todayIso(), [Validators.required]],
    endDate: this.fb.control<string | null>(null),
    type: this.fb.nonNullable.control<PlantingType>('Annual', [Validators.required]),
    plantCount: this.fb.control<number | null>(null, [Validators.min(1)]),
    notes: this.fb.control<string | null>(null, [Validators.maxLength(500)]),
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    blockId: [0, [Validators.required, Validators.min(1)]],
    cultivarId: [0, [Validators.required, Validators.min(1)]],
    startDate: ['', [Validators.required]],
    endDate: this.fb.control<string | null>(null),
    type: this.fb.nonNullable.control<PlantingType>('Annual', [Validators.required]),
    plantCount: this.fb.control<number | null>(null, [Validators.min(1)]),
    notes: this.fb.control<string | null>(null, [Validators.maxLength(500)]),
  });
  editError = signal('');

  ngOnInit(): void {
    this.loadLookups();
    this.load();
  }

  loadLookups(): void {
    forkJoin({
      blocks: this.blocksApi.getAll(true),
      crops: this.cropsApi.getAll(true),
      cultivars: this.cultivarsApi.getAll(true),
    }).subscribe(({ blocks, crops, cultivars }) => {
      this.blocks.set(blocks);
      this.crops.set(crops);
      this.cultivars.set(cultivars);
    });
  }

  load(): void {
    this.api.getAll().subscribe((rows) => {
      // Newest planting first - most relevant to someone recording today's work.
      this.plantings.set([...rows].sort((a, b) => b.startDate.localeCompare(a.startDate) || b.plantingId - a.plantingId));
    });
  }

  blockName(id: number): string {
    const block = this.blocksById().get(id);
    if (!block) return `#${id}`;
    return block.isActive ? block.name : `${block.name} (inactive)`;
  }

  cultivarLabel(id: number): string {
    const cultivar = this.cultivarsById().get(id);
    if (!cultivar) return `#${id}`;
    const crop = this.cropsById().get(cultivar.cropId);
    const cropName = crop ? crop.name : `crop #${cultivar.cropId}`;
    const label = `${cropName} - ${cultivar.name}`;
    return cultivar.isActive ? label : `${label} (inactive)`;
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreatePlantingRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({
          blockId: 0,
          cultivarId: 0,
          startDate: todayIso(),
          endDate: null,
          type: 'Annual',
          plantCount: null,
          notes: null,
        });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(planting: PlantingDto): void {
    this.editingId.set(planting.plantingId);
    this.editError.set('');
    this.editForm.setValue({
      blockId: planting.blockId,
      cultivarId: planting.cultivarId,
      startDate: planting.startDate.slice(0, 10),
      endDate: planting.endDate ? planting.endDate.slice(0, 10) : null,
      type: planting.type,
      plantCount: planting.plantCount,
      notes: planting.notes,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdatePlantingRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }
}
