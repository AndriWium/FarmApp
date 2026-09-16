import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { RecordCountsRequest, StartStockTakeRequest, StockTakeDto } from './stock-take.model';

@Injectable({ providedIn: 'root' })
export class StockTakesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/stock-takes`;

  getAll() {
    return this.http.get<StockTakeDto[]>(this.url);
  }

  getById(id: number) {
    return this.http.get<StockTakeDto>(`${this.url}/${id}`);
  }

  // Step 1: creates the StockTake header + one line per selected batch, SystemQty snapshotted
  // server-side. Never send CountedQty here - the backend owns it.
  start(req: StartStockTakeRequest) {
    return this.http.post<StockTakeDto>(this.url, req);
  }

  // Step 2: posts physical counts for a subset of this stock take's lines. Only ever call this
  // for lines whose CountedQty is still null (see stock-take-detail-page) - resubmitting a
  // line that already has a count would make the backend record a second Adjustment movement
  // for the same variance, silently double-adjusting on-hand.
  recordCounts(id: number, req: RecordCountsRequest) {
    return this.http.post<StockTakeDto>(`${this.url}/${id}/counts`, req);
  }
}
