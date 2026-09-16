import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateCultivarRequest, CultivarDto, UpdateCultivarRequest } from './cultivar.model';

@Injectable({ providedIn: 'root' })
export class CultivarsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/cultivars`;

  getAll(includeInactive = false) {
    return this.http.get<CultivarDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<CultivarDto>(`${this.url}/${id}`);
  }

  create(req: CreateCultivarRequest) {
    return this.http.post<CultivarDto>(this.url, req);
  }

  update(id: number, req: UpdateCultivarRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
