import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  ActivityTypeDto,
  CreateActivityTypeRequest,
  UpdateActivityTypeRequest,
} from './activity-type.model';

@Injectable({ providedIn: 'root' })
export class ActivityTypesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/activity-types`;

  getAll(includeInactive = false) {
    return this.http.get<ActivityTypeDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<ActivityTypeDto>(`${this.url}/${id}`);
  }

  create(req: CreateActivityTypeRequest) {
    return this.http.post<ActivityTypeDto>(this.url, req);
  }

  update(id: number, req: UpdateActivityTypeRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
