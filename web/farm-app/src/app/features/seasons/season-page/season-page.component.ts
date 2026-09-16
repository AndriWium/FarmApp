import { DecimalPipe } from '@angular/common';
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
import { PlantingDto } from '../../plantings/planting.model';
import { PlantingsApiService } from '../../plantings/plantings-api.service';
import { CreateSeasonRequest, SeasonDto, UpdateSeasonRequest } from '../season.model';
import { SeasonsApiService } from '../seasons-api.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

// Mirrors FarmApp.Domain.Services.SeasonCostCalculator.CalculateEstimatedCostPerKg exactly
// (ExpectedTotalCost / ExpectedYieldKg, rounded to 2dp) - a live client-side preview only; the
// authoritative value always comes back from the server on the SeasonDto after save.
function previewEstimatedCostPerKg(totalCost: number | null, yieldKg: number | null): number | null {
  if (totalCost === null || yieldKg === null || yieldKg <= 0) return null;
  return Math.round((totalCost / yieldKg) * 100) / 100;
}

@Component({
  selector: 'app-season-page',
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './season-page.component.html',
  styleUrl: './season-page.component.scss',
})
export class SeasonPageComponent implements OnInit {
  private api = inject(SeasonsApiService);
  private plantingsApi = inject(PlantingsApiService);
  private blocksApi = inject(BlocksApiService);
  private cropsApi = inject(CropsApiService);
  private cultivarsApi = inject(CultivarsApiService);
  private fb = inject(FormBuilder);

  // No canManage gate - SeasonsController carries no CanManageMasterData attribute on any verb
  // (verified directly; see DECISIONS.md Phase 3a, same treatment as PlantingsController).
  seasons = signal<SeasonDto[]>([]);

  plantings = signal<PlantingDto[]>([]);
  plantingsById = computed(() => new Map(this.plantings().map((p) => [p.plantingId, p])));
  blocks = signal<BlockDto[]>([]);
  blocksById = computed(() => new Map(this.blocks().map((b) => [b.blockId, b])));
  crops = signal<CropDto[]>([]);
  cropsById = computed(() => new Map(this.crops().map((c) => [c.cropId, c])));
  cultivars = signal<CultivarDto[]>([]);
  cultivarsById = computed(() => new Map(this.cultivars().map((c) => [c.cultivarId, c])));

  createForm = this.fb.nonNullable.group({
    plantingId: [0, [Validators.required, Validators.min(1)]],
    name: ['', [Validators.required, Validators.maxLength(50)]],
    startDate: [todayIso(), [Validators.required]],
    endDate: [todayIso(), [Validators.required]],
    expectedTotalCost: this.fb.control<number | null>(null, [Validators.min(0)]),
    expectedYieldKg: this.fb.control<number | null>(null, [Validators.min(0.001)]),
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    plantingId: [0, [Validators.required, Validators.min(1)]],
    name: ['', [Validators.required, Validators.maxLength(50)]],
    startDate: ['', [Validators.required]],
    endDate: ['', [Validators.required]],
    expectedTotalCost: this.fb.control<number | null>(null, [Validators.min(0)]),
    expectedYieldKg: this.fb.control<number | null>(null, [Validators.min(0.001)]),
  });
  editError = signal('');

  ngOnInit(): void {
    this.loadLookups();
    this.load();
  }

  loadLookups(): void {
    forkJoin({
      plantings: this.plantingsApi.getAll(),
      blocks: this.blocksApi.getAll(true),
      crops: this.cropsApi.getAll(true),
      cultivars: this.cultivarsApi.getAll(true),
    }).subscribe(({ plantings, blocks, crops, cultivars }) => {
      this.plantings.set(plantings);
      this.blocks.set(blocks);
      this.crops.set(crops);
      this.cultivars.set(cultivars);
    });
  }

  load(): void {
    this.api.getAll().subscribe((rows) => {
      this.seasons.set([...rows].sort((a, b) => b.startDate.localeCompare(a.startDate) || b.seasonId - a.seasonId));
    });
  }

  plantingLabel(id: number): string {
    const planting = this.plantingsById().get(id);
    if (!planting) return `#${id}`;
    const block = this.blocksById().get(planting.blockId);
    const cultivar = this.cultivarsById().get(planting.cultivarId);
    const crop = cultivar ? this.cropsById().get(cultivar.cropId) : undefined;
    const blockName = block ? block.name : `block #${planting.blockId}`;
    const cultivarName = cultivar
      ? `${crop ? crop.name : `crop #${cultivar.cropId}`} - ${cultivar.name}`
      : `cultivar #${planting.cultivarId}`;
    return `${blockName} / ${cultivarName} (started ${planting.startDate.slice(0, 10)})`;
  }

  createEstimatePreview(): number | null {
    const raw = this.createForm.getRawValue();
    return previewEstimatedCostPerKg(raw.expectedTotalCost, raw.expectedYieldKg);
  }

  editEstimatePreview(): number | null {
    const raw = this.editForm.getRawValue();
    return previewEstimatedCostPerKg(raw.expectedTotalCost, raw.expectedYieldKg);
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateSeasonRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({
          plantingId: 0,
          name: '',
          startDate: todayIso(),
          endDate: todayIso(),
          expectedTotalCost: null,
          expectedYieldKg: null,
        });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(season: SeasonDto): void {
    this.editingId.set(season.seasonId);
    this.editError.set('');
    this.editForm.setValue({
      plantingId: season.plantingId,
      name: season.name,
      startDate: season.startDate.slice(0, 10),
      endDate: season.endDate.slice(0, 10),
      expectedTotalCost: season.expectedTotalCost,
      expectedYieldKg: season.expectedYieldKg,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateSeasonRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }
}
