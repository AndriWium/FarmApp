import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { StockBatchDto } from './stock-batch.model';

@Injectable({ providedIn: 'root' })
export class StockBatchesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/stock-batches`;

  getAll() {
    return this.http.get<StockBatchDto[]>(this.url);
  }

  getById(id: number) {
    return this.http.get<StockBatchDto>(`${this.url}/${id}`);
  }

  // No bulk "on-hand per batch" endpoint exists (only the per-product/grade summary on
  // StockMovementsController) - the on-hand screen calls this once per batch via forkJoin. Fine
  // at this app's scale (a small farm's open batch count), per doc 11's "pragmatic" framing.
  getOnHand(id: number) {
    return this.http.get<number>(`${this.url}/${id}/on-hand`);
  }
}
