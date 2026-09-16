import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreatePackSizeRequest, PackSizeDto, UpdatePackSizeRequest } from './pack-size.model';

@Injectable({ providedIn: 'root' })
export class PackSizesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/pack-sizes`;

  // No includeInactive param here - PackSize has no IsActive column (see model comment); the
  // backend's GET returns every pack size that still exists, full stop.
  getAll() {
    return this.http.get<PackSizeDto[]>(this.url);
  }

  getById(id: number) {
    return this.http.get<PackSizeDto>(`${this.url}/${id}`);
  }

  create(req: CreatePackSizeRequest) {
    return this.http.post<PackSizeDto>(this.url, req);
  }

  update(id: number, req: UpdatePackSizeRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
