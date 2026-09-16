import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { BlockDto } from '../../blocks/block.model';
import { BlocksApiService } from '../../blocks/blocks-api.service';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { PlantingDto } from '../../plantings/planting.model';
import { PlantingsApiService } from '../../plantings/plantings-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { SeasonDto } from '../../seasons/season.model';
import { SeasonsApiService } from '../../seasons/seasons-api.service';
import { HarvestDto } from '../harvest.model';
import { HarvestsApiService } from '../harvests-api.service';

@Component({
  selector: 'app-harvest-detail-page',
  imports: [DatePipe, DecimalPipe, RouterLink],
  templateUrl: './harvest-detail-page.component.html',
  styleUrl: './harvest-detail-page.component.scss',
})
export class HarvestDetailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private api = inject(HarvestsApiService);
  private seasonsApi = inject(SeasonsApiService);
  private plantingsApi = inject(PlantingsApiService);
  private blocksApi = inject(BlocksApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);

  loading = signal(true);
  notFound = signal(false);
  harvest = signal<HarvestDto | null>(null);
  seasons = signal<SeasonDto[]>([]);
  plantings = signal<PlantingDto[]>([]);
  blocks = signal<BlockDto[]>([]);
  products = signal<ProductDto[]>([]);
  grades = signal<GradeDto[]>([]);

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
      harvest: this.api.getById(id),
      seasons: this.seasonsApi.getAll(),
      plantings: this.plantingsApi.getAll(),
      blocks: this.blocksApi.getAll(true),
      products: this.productsApi.getAll(true),
      grades: this.gradesApi.getAll(true),
    }).subscribe({
      next: ({ harvest, seasons, plantings, blocks, products, grades }) => {
        this.harvest.set(harvest);
        this.seasons.set(seasons);
        this.plantings.set(plantings);
        this.blocks.set(blocks);
        this.products.set(products);
        this.grades.set(grades);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
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

  productName(id: number): string {
    return this.products().find((p) => p.productId === id)?.name ?? `#${id}`;
  }

  gradeName(id: number | null): string {
    if (id === null) return '-';
    return this.grades().find((g) => g.gradeId === id)?.name ?? `#${id}`;
  }

  totalKg(harvest: HarvestDto): number {
    return harvest.lines.reduce((sum, l) => sum + l.qtyKg, 0);
  }
}
