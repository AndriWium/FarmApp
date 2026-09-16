import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateRoadmapItemRequest, RoadmapItemDto, UpdateRoadmapItemRequest } from './roadmap.model';

@Injectable({ providedIn: 'root' })
export class RoadmapApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/roadmapitems`;

  getAll() {
    return this.http.get<RoadmapItemDto[]>(this.url);
  }

  create(req: CreateRoadmapItemRequest) {
    return this.http.post<RoadmapItemDto>(this.url, req);
  }

  update(id: number, req: UpdateRoadmapItemRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }
}
