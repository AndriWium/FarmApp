import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { LocationDto } from '../../locations/location.model';
import { LocationsApiService } from '../../locations/locations-api.service';
import { StockTakeDto } from '../stock-take.model';
import { StockTakesApiService } from '../stock-takes-api.service';

@Component({
  selector: 'app-stock-take-list-page',
  imports: [DatePipe, RouterLink],
  templateUrl: './stock-take-list-page.component.html',
  styleUrl: './stock-take-list-page.component.scss',
})
export class StockTakeListPageComponent implements OnInit {
  private api = inject(StockTakesApiService);
  private locationsApi = inject(LocationsApiService);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  loading = signal(true);
  stockTakes = signal<StockTakeDto[]>([]);
  locations = signal<LocationDto[]>([]);

  sorted = computed(() =>
    [...this.stockTakes()].sort((a, b) => b.date.localeCompare(a.date) || b.stockTakeId - a.stockTakeId),
  );

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      stockTakes: this.api.getAll(),
      locations: this.locationsApi.getAll(true),
    }).subscribe(({ stockTakes, locations }) => {
      this.stockTakes.set(stockTakes);
      this.locations.set(locations);
      this.loading.set(false);
    });
  }

  locationName(id: number | null): string {
    if (id === null) return '-';
    const location = this.locations().find((l) => l.locationId === id);
    if (!location) return `#${id}`;
    return location.isActive ? location.name : `${location.name} (inactive)`;
  }

  countedLines(stockTake: StockTakeDto): number {
    return stockTake.lines.filter((l) => l.countedQty !== null).length;
  }

  isFullyCounted(stockTake: StockTakeDto): boolean {
    return stockTake.lines.length > 0 && this.countedLines(stockTake) === stockTake.lines.length;
  }
}
