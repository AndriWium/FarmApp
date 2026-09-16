import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateGradeRequest, GradeDto, UpdateGradeRequest } from './grade.model';

@Injectable({ providedIn: 'root' })
export class GradesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/grades`;

  getAll(includeInactive = false) {
    return this.http.get<GradeDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<GradeDto>(`${this.url}/${id}`);
  }

  create(req: CreateGradeRequest) {
    return this.http.post<GradeDto>(this.url, req);
  }

  update(id: number, req: UpdateGradeRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
