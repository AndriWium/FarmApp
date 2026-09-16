import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CloseTillSessionRequest, OpenTillSessionRequest, TillSessionDto } from './till-session.model';

@Injectable({ providedIn: 'root' })
export class TillSessionsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/till-sessions`;

  open(req: OpenTillSessionRequest) {
    return this.http.post<TillSessionDto>(this.url, req);
  }

  getById(id: number) {
    return this.http.get<TillSessionDto>(`${this.url}/${id}`);
  }

  getAll(locationId?: number | null, openOnly = false) {
    const params: Record<string, string | number | boolean> = { openOnly };
    if (locationId !== null && locationId !== undefined) params['locationId'] = locationId;
    return this.http.get<TillSessionDto[]>(this.url, { params });
  }

  // Day close (Phase 5d-2) - not used by this phase's sell screen, kept for a complete mirror.
  close(id: number, req: CloseTillSessionRequest) {
    return this.http.post<TillSessionDto>(`${this.url}/${id}/close`, req);
  }
}
