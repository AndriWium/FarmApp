import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, merge } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { BlockDto, WithholdingStatusDto } from '../../blocks/block.model';
import { BlocksApiService } from '../../blocks/blocks-api.service';
import { GradeDto } from '../../grades/grade.model';
import { GradesApiService } from '../../grades/grades-api.service';
import { PlantingDto } from '../../plantings/planting.model';
import { PlantingsApiService } from '../../plantings/plantings-api.service';
import { ProductDto } from '../../products/product.model';
import { ProductsApiService } from '../../products/products-api.service';
import { SeasonDto } from '../../seasons/season.model';
import { SeasonsApiService } from '../../seasons/seasons-api.service';
import { CreateHarvestLineRequest, HarvestDto } from '../harvest.model';
import { HarvestsApiService } from '../harvests-api.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

@Component({
  selector: 'app-harvest-form-page',
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe],
  templateUrl: './harvest-form-page.component.html',
  styleUrl: './harvest-form-page.component.scss',
})
export class HarvestFormPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(HarvestsApiService);
  private seasonsApi = inject(SeasonsApiService);
  private plantingsApi = inject(PlantingsApiService);
  private blocksApi = inject(BlocksApiService);
  private productsApi = inject(ProductsApiService);
  private gradesApi = inject(GradesApiService);
  private auth = inject(AuthService);

  // HarvestsController carries no CanManageMasterData attribute on any verb (verified directly) -
  // day-to-day farm capture, same footing as Planting/Season/RainfallLog/Activity. No canManage
  // gate. Withholding enforcement is a business rule inside HarvestService, not an auth concern -
  // any authenticated user can record a harvest, but a locked block still requires an override.

  seasons = signal<SeasonDto[]>([]);
  seasonsById = computed(() => new Map(this.seasons().map((s) => [s.seasonId, s])));
  plantings = signal<PlantingDto[]>([]);
  plantingsById = computed(() => new Map(this.plantings().map((p) => [p.plantingId, p])));
  blocks = signal<BlockDto[]>([]);
  blocksById = computed(() => new Map(this.blocks().map((b) => [b.blockId, b])));
  products = signal<ProductDto[]>([]);
  grades = signal<GradeDto[]>([]);

  header = this.fb.nonNullable.group({
    seasonId: [0, [Validators.required, Validators.min(1)]],
    date: [todayIso(), [Validators.required]],
    // PickedBy is a nullable FK to AppUser (Harvest.PickedBy, no navigation) - there is no
    // GET /users listing endpoint anywhere in this API (only /auth/login|refresh), so there is no
    // real dropdown to build. Pre-filled with the logged-in user's own AppUserId (decoded straight
    // off the JWT's NameIdentifier claim, confirmed to be AppUserId.ToString() in TokenService)
    // as a sensible default - the common case is the person logging the harvest also picked it -
    // but left as a plain editable number so a different picker's id can be typed in instead. See
    // DECISIONS.md for this judgment call.
    pickedBy: this.fb.control<number | null>(null, [Validators.min(1)]),
    notes: this.fb.control<string | null>(null, [Validators.maxLength(500)]),
  });

  lines = this.fb.array<ReturnType<typeof this.newLineGroup>>([]);

  // The core food-safety UX (doc 05 §5): recomputed the moment Season or Date changes, via
  // Season.PlantingId -> Planting.BlockId -> GET /blocks/{id}/withholding-status?date=. Shown
  // BEFORE the user has touched Submit at all - not just surfaced on a rejected submit.
  withholdingStatus = signal<WithholdingStatusDto | null>(null);
  checkingWithholding = signal(false);
  isLocked = computed(() => this.withholdingStatus()?.isLocked ?? false);
  overrideReason = new FormControl('', { nonNullable: true });

  submitting = signal(false);
  submitError = signal('');
  created = signal<HarvestDto | null>(null);

  ngOnInit(): void {
    const currentUserId = Number(this.auth.currentUser()?.userId);
    if (Number.isFinite(currentUserId) && currentUserId > 0) {
      this.header.controls.pickedBy.setValue(currentUserId);
    }

    forkJoin({
      seasons: this.seasonsApi.getAll(),
      plantings: this.plantingsApi.getAll(),
      blocks: this.blocksApi.getAll(true),
      products: this.productsApi.getAll(),
      grades: this.gradesApi.getAll(),
    }).subscribe(({ seasons, plantings, blocks, products, grades }) => {
      this.seasons.set(seasons);
      this.plantings.set(plantings);
      this.blocks.set(blocks);
      this.products.set(products);
      this.grades.set(grades);
    });

    this.addLine();

    // Proactive check, re-run on every Season or Date change - this is the whole point of the
    // feature, so it has to fire on its own, not wait for a button press.
    merge(this.header.controls.seasonId.valueChanges, this.header.controls.date.valueChanges)
      .pipe(debounceTime(150))
      .subscribe(() => this.checkWithholding());
  }

  private blockIdForSeason(seasonId: number): number | null {
    const season = this.seasonsById().get(seasonId);
    if (!season) return null;
    const planting = this.plantingsById().get(season.plantingId);
    return planting ? planting.blockId : null;
  }

  private checkWithholding(): void {
    const raw = this.header.getRawValue();
    this.withholdingStatus.set(null);
    this.overrideReason.setValue('');

    const blockId = this.blockIdForSeason(raw.seasonId);
    if (!blockId || !raw.date) return;

    this.checkingWithholding.set(true);
    this.blocksApi.getWithholdingStatus(blockId, raw.date).subscribe({
      next: (status) => {
        this.checkingWithholding.set(false);
        this.withholdingStatus.set(status);
      },
      error: () => {
        this.checkingWithholding.set(false);
        this.withholdingStatus.set(null);
      },
    });
  }

  blockNameForSeason(seasonId: number): string {
    const blockId = this.blockIdForSeason(seasonId);
    if (!blockId) return '?';
    return this.blocksById().get(blockId)?.name ?? `#${blockId}`;
  }

  private newLineGroup() {
    return this.fb.nonNullable.group({
      productId: [0, [Validators.required, Validators.min(1)]],
      gradeId: this.fb.control<number | null>(null),
      qtyKg: [0, [Validators.required, Validators.min(0.001)]],
      shelfLifeDays: [7, [Validators.required, Validators.min(1)]],
    });
  }

  addLine(): void {
    this.lines.push(this.newLineGroup());
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  seasonLabel(id: number): string {
    const season = this.seasonsById().get(id);
    if (!season) return `#${id}`;
    return `${season.name} (${this.blockNameForSeason(id)})`;
  }

  productName(id: number): string {
    return this.products().find((p) => p.productId === id)?.name ?? `#${id}`;
  }

  gradeName(id: number | null): string {
    if (id === null) return '-';
    return this.grades().find((g) => g.gradeId === id)?.name ?? `#${id}`;
  }

  // Submit is disabled outright while locked and no override reason has been typed - "the button
  // physically won't work", not a silent failure discovered after the fact (task brief).
  canSubmit(): boolean {
    if (this.header.invalid || this.lines.invalid || this.lines.length === 0 || this.submitting()) return false;
    if (this.isLocked() && !this.overrideReason.value.trim()) return false;
    return true;
  }

  submit(): void {
    if (!this.canSubmit()) {
      this.header.markAllAsTouched();
      this.lines.markAllAsTouched();
      this.overrideReason.markAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set('');
    this.created.set(null);

    const headerRaw = this.header.getRawValue();
    const lineRequests: CreateHarvestLineRequest[] = this.lines.controls.map((c) => c.getRawValue());

    this.api
      .create({
        seasonId: headerRaw.seasonId,
        date: headerRaw.date,
        pickedBy: headerRaw.pickedBy,
        notes: headerRaw.notes,
        withholdingOverrideReason: this.isLocked() ? this.overrideReason.value.trim() : null,
        lines: lineRequests,
      })
      .subscribe({
        next: (harvest) => {
          this.submitting.set(false);
          this.created.set(harvest);
          this.resetForm();
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          const detail = extractErrorMessage(err);
          this.submitError.set(detail);

          // The real safety net for the race the task brief calls out (season/date changed
          // between the proactive check and submit): the backend's own WithholdingLocked
          // rejection is handled the same way as a proactively-discovered lock, not as a generic
          // error - the banner appears (from the server's own detail text) and the override field
          // is revealed, rather than leaving the farmer looking at a bare error string.
          const title = (err.error as { title?: string } | null)?.title;
          if (err.status === 409 && title === 'Withholding period active') {
            this.withholdingStatus.set({ isLocked: true, lockedUntil: null, reason: detail });
            // Re-run the real proactive check too, so LockedUntil gets filled in properly once the
            // authoritative status is back, rather than staying null forever.
            this.checkWithholding();
          }
        },
      });
  }

  private resetForm(): void {
    const currentUserId = Number(this.auth.currentUser()?.userId);
    this.header.reset({
      seasonId: 0,
      date: todayIso(),
      pickedBy: Number.isFinite(currentUserId) && currentUserId > 0 ? currentUserId : null,
      notes: null,
    });
    this.lines.clear();
    this.addLine();
    this.withholdingStatus.set(null);
    this.overrideReason.setValue('');
  }
}
