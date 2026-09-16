import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { BlockDto } from '../../blocks/block.model';
import { BlocksApiService } from '../../blocks/blocks-api.service';
import { PlantingDto } from '../../plantings/planting.model';
import { PlantingsApiService } from '../../plantings/plantings-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { SeasonDto } from '../../seasons/season.model';
import { SeasonsApiService } from '../../seasons/seasons-api.service';
import { HarvestDto } from '../harvest.model';
import { HarvestsApiService } from '../harvests-api.service';

@Component({
  selector: 'app-harvest-list-page',
  imports: [DatePipe, RouterLink, FormsModule],
  templateUrl: './harvest-list-page.component.html',
  styleUrl: './harvest-list-page.component.scss',
})
export class HarvestListPageComponent implements OnInit {
  private api = inject(HarvestsApiService);
  private seasonsApi = inject(SeasonsApiService);
  private plantingsApi = inject(PlantingsApiService);
  private blocksApi = inject(BlocksApiService);
  private productsApi = inject(ProductsApiService);

  loading = signal(true);
  harvests = signal<HarvestDto[]>([]);

  seasons = signal<SeasonDto[]>([]);
  seasonsById = computed(() => new Map(this.seasons().map((s) => [s.seasonId, s])));
  plantings = signal<PlantingDto[]>([]);
  plantingsById = computed(() => new Map(this.plantings().map((p) => [p.plantingId, p])));
  blocks = signal<BlockDto[]>([]);
  blocksById = computed(() => new Map(this.blocks().map((b) => [b.blockId, b])));
  products = signal<ProductDto[]>([]);
  productsById = computed(() => new Map(this.products().map((p) => [p.productId, p])));

  seasonFilter = signal(0);
  filteredHarvests = computed(() => {
    const filter = this.seasonFilter();
    const rows = this.harvests();
    const filtered = filter === 0 ? rows : rows.filter((h) => h.seasonId === filter);
    return [...filtered].sort((a, b) => b.date.localeCompare(a.date) || b.harvestId - a.harvestId);
  });

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin({
      harvests: this.api.getAll(),
      seasons: this.seasonsApi.getAll(),
      plantings: this.plantingsApi.getAll(),
      blocks: this.blocksApi.getAll(true),
      products: this.productsApi.getAll(true),
    }).subscribe(({ harvests, seasons, plantings, blocks, products }) => {
      this.harvests.set(harvests);
      this.seasons.set(seasons);
      this.plantings.set(plantings);
      this.blocks.set(blocks);
      this.products.set(products);
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

  totalKg(harvest: HarvestDto): number {
    return harvest.lines.reduce((sum, l) => sum + l.qtyKg, 0);
  }

  productSummary(harvest: HarvestDto): string {
    const names = [...new Set(harvest.lines.map((l) => this.productsById().get(l.productId)?.name ?? `#${l.productId}`))];
    return names.join(', ');
  }
}
