import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateInputItemRequest, InputItemDto, UpdateInputItemRequest } from './input-item.model';

@Injectable({ providedIn: 'root' })
export class InputItemsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/input-items`;

  getAll(includeInactive = false) {
    return this.http.get<InputItemDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<InputItemDto>(`${this.url}/${id}`);
  }

  create(req: CreateInputItemRequest) {
    return this.http.post<InputItemDto>(this.url, req);
  }

  update(id: number, req: UpdateInputItemRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  // What ActivityInput.UnitCost snapshots at consumption time (InputItemsController) - shown next
  // to each recipe line in the product recipe editor so the farmer sees roughly what a batch
  // costs before committing (Phase 5c-1 task brief, nice-to-have).
  getWeightedAverageCost(id: number) {
    return this.http.get<number>(`${this.url}/${id}/weighted-average-cost`);
  }
}
