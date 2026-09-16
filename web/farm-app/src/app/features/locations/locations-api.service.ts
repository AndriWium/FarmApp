import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateLocationRequest, LocationDto, UpdateLocationRequest } from './location.model';

@Injectable({ providedIn: 'root' })
export class LocationsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/locations`;

  getAll(includeInactive = false) {
    return this.http.get<LocationDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<LocationDto>(`${this.url}/${id}`);
  }

  create(req: CreateLocationRequest) {
    return this.http.post<LocationDto>(this.url, req);
  }

  update(id: number, req: UpdateLocationRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
