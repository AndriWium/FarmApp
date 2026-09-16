import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateCropRequest, CropDto, UpdateCropRequest } from './crop.model';

@Injectable({ providedIn: 'root' })
export class CropsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/crops`;

  getAll(includeInactive = false) {
    return this.http.get<CropDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<CropDto>(`${this.url}/${id}`);
  }

  create(req: CreateCropRequest) {
    return this.http.post<CropDto>(this.url, req);
  }

  update(id: number, req: UpdateCropRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
