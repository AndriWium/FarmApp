import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { ActivityTypeDto } from '../../activity-types/activity-type.model';
import { ActivityTypesApiService } from '../../activity-types/activity-types-api.service';
import { InputItemDto } from '../../input-items/input-item.model';
import { InputItemsApiService } from '../../input-items/input-items-api.service';
import { BlockDto } from '../../blocks/block.model';
import { BlocksApiService } from '../../blocks/blocks-api.service';
import { PlantingDto } from '../../plantings/planting.model';
import { PlantingsApiService } from '../../plantings/plantings-api.service';
import { SeasonDto } from '../../seasons/season.model';
import { SeasonsApiService } from '../../seasons/seasons-api.service';
import { ActivityDto, CreateActivityInputLineRequest } from '../activity.model';
import { ActivitiesApiService } from '../activities-api.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

// Pulled straight out of ActivityService.CreateActivityAsync's rejection message:
// `$"Input item {line.InputItemId}: requested {line.Qty}, only {onHand} on hand."` - parsed back
// out so the offending line can be highlighted in the UI (same intent as PosPageComponent's
// flagShortLines, but here the backend already names the exact InputItemId, no re-derivation
// against on-hand needed).
const INSUFFICIENT_STOCK_ITEM_RE = /Input item (\d+):/;

@Component({
  selector: 'app-activity-form-page',
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe],
  templateUrl: './activity-form-page.component.html',
  styleUrl: './activity-form-page.component.scss',
})
export class ActivityFormPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(ActivitiesApiService);
  private seasonsApi = inject(SeasonsApiService);
  private plantingsApi = inject(PlantingsApiService);
  private blocksApi = inject(BlocksApiService);
  private activityTypesApi = inject(ActivityTypesApiService);
  private inputItemsApi = inject(InputItemsApiService);

  // ActivitiesController carries no CanManageMasterData attribute on any verb (verified directly) -
  // day-to-day farm capture, same footing as Planting/Season/RainfallLog. No canManage gate.

  seasons = signal<SeasonDto[]>([]);
  plantings = signal<PlantingDto[]>([]);
  plantingsById = computed(() => new Map(this.plantings().map((p) => [p.plantingId, p])));
  blocks = signal<BlockDto[]>([]);
  blocksById = computed(() => new Map(this.blocks().map((b) => [b.blockId, b])));
  activityTypes = signal<ActivityTypeDto[]>([]);
  inputItems = signal<InputItemDto[]>([]);
  inputItemsById = computed(() => new Map(this.inputItems().map((i) => [i.inputItemId, i])));

  header = this.fb.nonNullable.group({
    seasonId: [0, [Validators.required, Validators.min(1)]],
    activityTypeId: [0, [Validators.required, Validators.min(1)]],
    date: [todayIso(), [Validators.required]],
    labourHours: [0, [Validators.required, Validators.min(0)]],
    labourCost: [0, [Validators.required, Validators.min(0)]],
    notes: this.fb.control<string | null>(null, [Validators.maxLength(500)]),
  });

  // Not every activity consumes input stock (pruning, weeding, irrigation) - unlike
  // InputPurchase/Harvest, zero lines is a perfectly valid activity, so this array may stay empty.
  lines = this.fb.array<ReturnType<typeof this.newLineGroup>>([]);

  submitting = signal(false);
  submitError = signal('');
  created = signal<ActivityDto | null>(null);
  insufficientStockLineIndexes = signal<Set<number>>(new Set());
  // InputItemId -> on-hand right after a successful create, so the form doubles as proof stock
  // actually depleted (same InputPurchaseFormPageComponent precedent).
  onHandByItemId = signal<Map<number, number>>(new Map());

  ngOnInit(): void {
    forkJoin({
      seasons: this.seasonsApi.getAll(),
      plantings: this.plantingsApi.getAll(),
      blocks: this.blocksApi.getAll(true),
      activityTypes: this.activityTypesApi.getAll(),
      inputItems: this.inputItemsApi.getAll(),
    }).subscribe(({ seasons, plantings, blocks, activityTypes, inputItems }) => {
      this.seasons.set(seasons);
      this.plantings.set(plantings);
      this.blocks.set(blocks);
      this.activityTypes.set(activityTypes);
      this.inputItems.set(inputItems);
    });
  }

  private newLineGroup() {
    return this.fb.nonNullable.group({
      inputItemId: [0, [Validators.required, Validators.min(1)]],
      qty: [0, [Validators.required, Validators.min(0.001)]],
    });
  }

  addLine(): void {
    this.lines.push(this.newLineGroup());
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
    this.insufficientStockLineIndexes.update((keys) => {
      if (keys.size === 0) return keys;
      const next = new Set<number>();
      for (const k of keys) {
        if (k < index) next.add(k);
        else if (k > index) next.add(k - 1);
      }
      return next;
    });
  }

  seasonLabel(id: number): string {
    const season = this.seasons().find((s) => s.seasonId === id);
    if (!season) return `#${id}`;
    const planting = this.plantingsById().get(season.plantingId);
    const block = planting ? this.blocksById().get(planting.blockId) : undefined;
    const blockName = block ? block.name : planting ? `block #${planting.blockId}` : '?';
    return `${season.name} (${blockName})`;
  }

  submit(): void {
    if (this.header.invalid || this.lines.invalid) {
      this.header.markAllAsTouched();
      this.lines.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set('');
    this.insufficientStockLineIndexes.set(new Set());
    this.created.set(null);
    this.onHandByItemId.set(new Map());

    const headerRaw = this.header.getRawValue();
    const inputRequests: CreateActivityInputLineRequest[] = this.lines.controls.map((c) => c.getRawValue());

    this.api
      .create({
        seasonId: headerRaw.seasonId,
        activityTypeId: headerRaw.activityTypeId,
        date: headerRaw.date,
        labourHours: headerRaw.labourHours,
        labourCost: headerRaw.labourCost,
        notes: headerRaw.notes,
        inputs: inputRequests,
      })
      .subscribe({
        next: (activity) => {
          this.submitting.set(false);
          this.created.set(activity);
          this.loadOnHand(activity);
          this.resetForm();
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          const detail = extractErrorMessage(err);
          this.submitError.set(detail);

          const title = (err.error as { title?: string } | null)?.title;
          if (err.status === 409 && title === 'Insufficient stock') {
            const match = detail.match(INSUFFICIENT_STOCK_ITEM_RE);
            if (match) {
              const badItemId = Number(match[1]);
              const badIndexes = new Set<number>();
              this.lines.controls.forEach((c, i) => {
                if (c.getRawValue().inputItemId === badItemId) badIndexes.add(i);
              });
              this.insufficientStockLineIndexes.set(badIndexes);
            }
          }
          // Form data (header + lines) is deliberately left untouched on any error - only a
          // genuine create success resets it (task brief: "without losing the rest of the form's
          // data").
        },
      });
  }

  private loadOnHand(activity: ActivityDto): void {
    const itemIds = [...new Set(activity.inputs.map((l) => l.inputItemId))];
    if (itemIds.length === 0) return;
    forkJoin(itemIds.map((id) => this.inputItemsApi.getOnHand(id))).subscribe((onHands) => {
      this.onHandByItemId.set(new Map(itemIds.map((id, i) => [id, onHands[i]])));
    });
  }

  private resetForm(): void {
    this.header.reset({
      seasonId: 0,
      activityTypeId: 0,
      date: todayIso(),
      labourHours: 0,
      labourCost: 0,
      notes: null,
    });
    this.lines.clear();
    this.insufficientStockLineIndexes.set(new Set());
  }

  inputItemName(id: number): string {
    const item = this.inputItemsById().get(id);
    return item ? item.name : `#${id}`;
  }

  inputItemUnit(id: number): string {
    const item = this.inputItemsById().get(id);
    return item ? item.unit : '';
  }
}
