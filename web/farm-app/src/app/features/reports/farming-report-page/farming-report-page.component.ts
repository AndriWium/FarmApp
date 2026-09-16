import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { BlockDto } from '../../blocks/block.model';
import { BlocksApiService } from '../../blocks/blocks-api.service';
import { PlantingDto } from '../../plantings/planting.model';
import { PlantingsApiService } from '../../plantings/plantings-api.service';
import { SeasonDto } from '../../seasons/season.model';
import { SeasonsApiService } from '../../seasons/seasons-api.service';
import { HarvestSummaryRowDto, InputUsageRowDto, RainfallComparisonDto, SeasonFarmingReportDto } from '../report.model';
import { ReportsApiService } from '../reports-api.service';

function currentYearMonth(): { year: number; month: number } {
  const now = new Date();
  return { year: now.getFullYear(), month: now.getMonth() + 1 };
}

/**
 * Farming report (doc 04 §4) - the "should I plant this again?" table: per-season cost/kg
 * harvested/revenue attributed/margin, harvest summary vs last season, input usage, and rainfall
 * vs historical average for a chosen month (independent of the season picker - rainfall is a
 * calendar-month figure, doc 04, not a season-scoped one).
 */
@Component({
  selector: 'app-farming-report-page',
  imports: [DecimalPipe, FormsModule, RouterLink],
  templateUrl: './farming-report-page.component.html',
  styleUrl: './farming-report-page.component.scss',
})
export class FarmingReportPageComponent implements OnInit {
  private api = inject(ReportsApiService);
  private seasonsApi = inject(SeasonsApiService);
  private plantingsApi = inject(PlantingsApiService);
  private blocksApi = inject(BlocksApiService);

  seasons = signal<SeasonDto[]>([]);
  plantings = signal<PlantingDto[]>([]);
  plantingsById = computed(() => new Map(this.plantings().map((p) => [p.plantingId, p])));
  blocks = signal<BlockDto[]>([]);
  blocksById = computed(() => new Map(this.blocks().map((b) => [b.blockId, b])));

  selectedSeasonId = signal<number | null>(null);
  loadingSeason = signal(false);
  seasonError = signal('');

  farmingReport = signal<SeasonFarmingReportDto | null>(null);
  harvestSummary = signal<HarvestSummaryRowDto[]>([]);
  inputUsage = signal<InputUsageRowDto[]>([]);

  private defaultYearMonth = currentYearMonth();
  rainfallYear = signal(this.defaultYearMonth.year);
  rainfallMonth = signal(this.defaultYearMonth.month);
  rainfall = signal<RainfallComparisonDto | null>(null);
  loadingRainfall = signal(false);

  seasonMargin = computed(() => this.farmingReport()?.margin ?? null);

  ngOnInit(): void {
    forkJoin({
      seasons: this.seasonsApi.getAll(),
      plantings: this.plantingsApi.getAll(),
      blocks: this.blocksApi.getAll(true),
    }).subscribe(({ seasons, plantings, blocks }) => {
      const sorted = [...seasons].sort((a, b) => b.startDate.localeCompare(a.startDate) || b.seasonId - a.seasonId);
      this.seasons.set(sorted);
      this.plantings.set(plantings);
      this.blocks.set(blocks);
      if (sorted.length > 0) this.selectSeason(sorted[0].seasonId);
    });

    this.loadRainfall();
  }

  selectSeason(seasonId: number): void {
    this.selectedSeasonId.set(seasonId);
    this.loadingSeason.set(true);
    this.seasonError.set('');
    forkJoin({
      report: this.api.getSeasonFarmingReport(seasonId),
      harvestSummary: this.api.getHarvestSummary(seasonId),
      inputUsage: this.api.getInputUsage(seasonId),
    }).subscribe({
      next: ({ report, harvestSummary, inputUsage }) => {
        this.farmingReport.set(report);
        this.harvestSummary.set(harvestSummary);
        this.inputUsage.set(inputUsage);
        this.loadingSeason.set(false);
      },
      error: () => {
        this.seasonError.set('Could not load the farming report for this season.');
        this.loadingSeason.set(false);
      },
    });
  }

  loadRainfall(): void {
    this.loadingRainfall.set(true);
    this.api.getRainfall(this.rainfallYear(), this.rainfallMonth()).subscribe({
      next: (r) => {
        this.rainfall.set(r);
        this.loadingRainfall.set(false);
      },
      error: () => {
        this.rainfall.set(null);
        this.loadingRainfall.set(false);
      },
    });
  }

  seasonLabel(season: SeasonDto): string {
    const planting = this.plantingsById().get(season.plantingId);
    const block = planting ? this.blocksById().get(planting.blockId) : undefined;
    const blockName = block ? block.name : planting ? `block #${planting.blockId}` : '?';
    return `${season.name} (${blockName}) - ${season.status}`;
  }

  rainfallDiff(): number | null {
    const r = this.rainfall();
    if (!r || r.historicalAverageMm === null) return null;
    return r.thisPeriodMm - r.historicalAverageMm;
  }
}
