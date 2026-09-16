import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ActivityTypeDto } from '../../activity-types/activity-type.model';
import { ActivityTypesApiService } from '../../activity-types/activity-types-api.service';
import { BlockDto } from '../../blocks/block.model';
import { BlocksApiService } from '../../blocks/blocks-api.service';
import { PlantingDto } from '../../plantings/planting.model';
import { PlantingsApiService } from '../../plantings/plantings-api.service';
import { SeasonDto } from '../../seasons/season.model';
import { SeasonsApiService } from '../../seasons/seasons-api.service';
import { ActivityDto } from '../activity.model';
import { ActivitiesApiService } from '../activities-api.service';

@Component({
  selector: 'app-activity-list-page',
  imports: [DatePipe, RouterLink, FormsModule],
  templateUrl: './activity-list-page.component.html',
  styleUrl: './activity-list-page.component.scss',
})
export class ActivityListPageComponent implements OnInit {
  private api = inject(ActivitiesApiService);
  private seasonsApi = inject(SeasonsApiService);
  private plantingsApi = inject(PlantingsApiService);
  private blocksApi = inject(BlocksApiService);
  private activityTypesApi = inject(ActivityTypesApiService);

  loading = signal(true);
  activities = signal<ActivityDto[]>([]);

  seasons = signal<SeasonDto[]>([]);
  seasonsById = computed(() => new Map(this.seasons().map((s) => [s.seasonId, s])));
  plantings = signal<PlantingDto[]>([]);
  plantingsById = computed(() => new Map(this.plantings().map((p) => [p.plantingId, p])));
  blocks = signal<BlockDto[]>([]);
  blocksById = computed(() => new Map(this.blocks().map((b) => [b.blockId, b])));
  activityTypes = signal<ActivityTypeDto[]>([]);
  activityTypesById = computed(() => new Map(this.activityTypes().map((t) => [t.activityTypeId, t])));

  // Client-side filter, same treatment as InputPurchaseListPageComponent - the whole farm's
  // activity log is small enough that fetching everything and narrowing in the browser is fine
  // (ActivitiesController.GetAll(seasonId) exists too, wired up here instead since a farmer
  // switching the filter shouldn't refetch the whole lookup set each time).
  seasonFilter = signal(0);
  filteredActivities = computed(() => {
    const filter = this.seasonFilter();
    const rows = this.activities();
    const filtered = filter === 0 ? rows : rows.filter((a) => a.seasonId === filter);
    return [...filtered].sort((a, b) => b.date.localeCompare(a.date) || b.activityId - a.activityId);
  });

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin({
      activities: this.api.getAll(),
      seasons: this.seasonsApi.getAll(),
      plantings: this.plantingsApi.getAll(),
      blocks: this.blocksApi.getAll(true),
      activityTypes: this.activityTypesApi.getAll(true),
    }).subscribe(({ activities, seasons, plantings, blocks, activityTypes }) => {
      this.activities.set(activities);
      this.seasons.set(seasons);
      this.plantings.set(plantings);
      this.blocks.set(blocks);
      this.activityTypes.set(activityTypes);
      this.loading.set(false);
    });
  }

  seasonLabel(id: number): string {
    const season = this.seasonsById().get(id);
    if (!season) return `#${id}`;
    const planting = this.plantingsById().get(season.plantingId);
    const block = planting ? this.blocksById().get(planting.blockId) : undefined;
    const blockName = block ? block.name : planting ? `block #${planting.blockId}` : '?';
    return `${season.name} (${blockName})`;
  }

  activityTypeName(id: number): string {
    return this.activityTypesById().get(id)?.name ?? `#${id}`;
  }

  inputCount(activity: ActivityDto): number {
    return activity.inputs.length;
  }
}
