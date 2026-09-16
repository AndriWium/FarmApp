import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  RecordStockMovementRequest,
  SimpleMovementKind,
  StockMovementDto,
  StockOnHandSummaryDto,
  TransferStockRequest,
} from './stock-movement.model';

@Injectable({ providedIn: 'root' })
export class StockMovementsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/stock-movements`;

  record(kind: SimpleMovementKind, req: RecordStockMovementRequest) {
    return this.http.post<StockMovementDto[]>(`${this.url}/${kind}`, req);
  }

  transfer(req: TransferStockRequest) {
    return this.http.post<StockMovementDto[]>(`${this.url}/transfer`, req);
  }

  getOnHandSummary() {
    return this.http.get<StockOnHandSummaryDto[]>(`${this.url}/on-hand`);
  }
}
