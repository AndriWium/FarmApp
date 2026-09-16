import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { BlockDto, CreateBlockRequest, UpdateBlockRequest, WithholdingStatusDto } from './block.model';

@Injectable({ providedIn: 'root' })
export class BlocksApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/blocks`;

  getAll(includeInactive = false) {
    return this.http.get<BlockDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<BlockDto>(`${this.url}/${id}`);
  }

  create(req: CreateBlockRequest) {
    return this.http.post<BlockDto>(this.url, req);
  }

  update(id: number, req: UpdateBlockRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  // Read-only pre-check (doc 05 §5, BlocksController.GetWithholdingStatus) - lets the harvest
  // form warn about a chemical withholding lock BEFORE the farmer submits, not just on rejection.
  // date is an ISO yyyy-MM-dd string (ASP.NET model-binds it straight to DateTime? server-side).
  getWithholdingStatus(blockId: number, date: string) {
    return this.http.get<WithholdingStatusDto>(`${this.url}/${blockId}/withholding-status`, {
      params: { date },
    });
  }
}
