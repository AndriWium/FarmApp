import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreatePriceListRequest, PriceListDto, UpdatePriceListRequest } from './price-list.model';

@Injectable({ providedIn: 'root' })
export class PriceListsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/price-lists`;

  getAll(includeInactive = false) {
    return this.http.get<PriceListDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<PriceListDto>(`${this.url}/${id}`);
  }

  create(req: CreatePriceListRequest) {
    return this.http.post<PriceListDto>(this.url, req);
  }

  update(id: number, req: UpdatePriceListRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
