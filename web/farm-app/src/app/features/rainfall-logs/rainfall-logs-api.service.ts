import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateRainfallLogRequest, RainfallLogDto, UpdateRainfallLogRequest } from './rainfall-log.model';

@Injectable({ providedIn: 'root' })
export class RainfallLogsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/rainfall-logs`;

  // from/to (DateOnly, "yyyy-MM-dd") narrow the range server-side (RainfallLogsController.GetAll).
  getAll(from?: string, to?: string) {
    const params: Record<string, string> = {};
    if (from) params['from'] = from;
    if (to) params['to'] = to;
    return this.http.get<RainfallLogDto[]>(this.url, { params });
  }

  getById(id: number) {
    return this.http.get<RainfallLogDto>(`${this.url}/${id}`);
  }

  create(req: CreateRainfallLogRequest) {
    return this.http.post<RainfallLogDto>(this.url, req);
  }

  update(id: number, req: UpdateRainfallLogRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }
}
