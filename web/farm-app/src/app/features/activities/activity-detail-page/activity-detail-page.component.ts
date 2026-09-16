import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ActivityTypeDto } from '../../activity-types/activity-type.model';
import { ActivityTypesApiService } from '../../activity-types/activity-types-api.service';
import { BlockDto } from '../../blocks/block.model';
import { BlocksApiService } from '../../blocks/blocks-api.service';
import { InputItemDto } from '../../input-items/input-item.model';
import { InputItemsApiService } from '../../input-items/input-items-api.service';
import { PlantingDto } from '../../plantings/planting.model';
import { PlantingsApiService } from '../../plantings/plantings-api.service';
import { SeasonDto } from '../../seasons/season.model';
import { SeasonsApiService } from '../../seasons/seasons-api.service';
import { ActivityDto } from '../activity.model';
import { ActivitiesApiService } from '../activities-api.service';

@Component({
  selector: 'app-activity-detail-page',
  imports: [DatePipe, DecimalPipe, RouterLink],
  templateUrl: './activity-detail-page.component.html',
  styleUrl: './activity-detail-page.component.scss',
})
export class ActivityDetailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private api = inject(ActivitiesApiService);
  private seasonsApi = inject(SeasonsApiService);
  private plantingsApi = inject(PlantingsApiService);
  private blocksApi = inject(BlocksApiService);
  private activityTypesApi = inject(ActivityTypesApiService);
  private inputItemsApi = inject(InputItemsApiService);

  loading = signal(true);
  notFound = signal(false);
  activity = signal<ActivityDto | null>(null);
  seasons = signal<SeasonDto[]>([]);
  plantings = signal<PlantingDto[]>([]);
  blocks = signal<BlockDto[]>([]);
  activityTypes = signal<ActivityTypeDto[]>([]);
  inputItems = signal<InputItemDto[]>([]);
  onHandByItemId = signal<Map<number, number>>(new Map());

  // Subscribes to paramMap rather than a one-shot route.snapshot read - same route-reuse fix as
  // InputPurchaseDetailPageComponent/ProductRecipePageComponent (DECISIONS.md Phase 5c-1).
  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      if (!id) {
        this.notFound.set(true);
        this.loading.set(false);
        return;
      }
      this.load(id);
    });
  }

  private load(id: number): void {
    this.loading.set(true);
    this.notFound.set(false);
    forkJoin({
      activity: this.api.getById(id),
      seasons: this.seasonsApi.getAll(),
      plantings: this.plantingsApi.getAll(),
      blocks: this.blocksApi.getAll(true),
      activityTypes: this.activityTypesApi.getAll(true),
      inputItems: this.inputItemsApi.getAll(true),
    }).subscribe({
      next: ({ activity, seasons, plantings, blocks, activityTypes, inputItems }) => {
        this.activity.set(activity);
        this.seasons.set(seasons);
        this.plantings.set(plantings);
        this.blocks.set(blocks);
        this.activityTypes.set(activityTypes);
        this.inputItems.set(inputItems);
        this.loading.set(false);
        this.loadOnHand(activity);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
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

  seasonLabel(id: number): string {
    const season = this.seasons().find((s) => s.seasonId === id);
    if (!season) return `#${id}`;
    const planting = this.plantings().find((p) => p.plantingId === season.plantingId);
    const block = planting ? this.blocks().find((b) => b.blockId === planting.blockId) : undefined;
    const blockName = block ? block.name : planting ? `block #${planting.blockId}` : '?';
    return `${season.name} (${blockName})`;
  }

  activityTypeName(id: number): string {
    const type = this.activityTypes().find((t) => t.activityTypeId === id);
    return type ? `${type.name} (${type.category})` : `#${id}`;
  }

  inputItemName(id: number): string {
    const item = this.inputItems().find((i) => i.inputItemId === id);
    return item ? item.name : `#${id}`;
  }

  inputItemUnit(id: number): string {
    const item = this.inputItems().find((i) => i.inputItemId === id);
    return item ? item.unit : '';
  }
}
