import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateSaleRequest, RefundSaleRequest, SaleDto } from './sale.model';

@Injectable({ providedIn: 'root' })
export class SalesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/sales`;

  // observe: 'response' (not the default body-only) so the caller can tell a genuine create (201)
  // apart from an idempotent ClientGuid replay (200) - doc 08's own distinction, and the one thing
  // a plain Observable<SaleDto> can't answer on its own.
  create(req: CreateSaleRequest) {
    return this.http.post<SaleDto>(this.url, req, { observe: 'response' });
  }

  getById(id: number) {
    return this.http.get<SaleDto>(`${this.url}/${id}`);
  }

  getAll(tillSessionId?: number | null) {
    const params: Record<string, number> = {};
    if (tillSessionId !== null && tillSessionId !== undefined) params['tillSessionId'] = tillSessionId;
    return this.http.get<SaleDto[]>(this.url, { params });
  }

  // Refunds (Phase 5d-2) - not used by this phase's sell screen, kept for a complete mirror.
  refund(id: number, req: RefundSaleRequest) {
    return this.http.post<SaleDto>(`${this.url}/${id}/refund`, req);
  }
}
