import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateSeasonRequest, SeasonDto, UpdateSeasonRequest } from './season.model';

@Injectable({ providedIn: 'root' })
export class SeasonsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/seasons`;

  getAll(plantingId?: number) {
    return this.http.get<SeasonDto[]>(this.url, plantingId ? { params: { plantingId } } : {});
  }

  getById(id: number) {
    return this.http.get<SeasonDto>(`${this.url}/${id}`);
  }

  create(req: CreateSeasonRequest) {
    return this.http.post<SeasonDto>(this.url, req);
  }

  update(id: number, req: UpdateSeasonRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  // Cost-preview/close (doc 09's true-up flow) deliberately not wired up here - that's a separate,
  // more involved screen, out of scope for Phase 5e-1 (task brief).
}
