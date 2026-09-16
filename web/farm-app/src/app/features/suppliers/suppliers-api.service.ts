import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateSupplierRequest, SupplierDto, UpdateSupplierRequest } from './supplier.model';

@Injectable({ providedIn: 'root' })
export class SuppliersApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/suppliers`;

  getAll(includeInactive = false) {
    return this.http.get<SupplierDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<SupplierDto>(`${this.url}/${id}`);
  }

  create(req: CreateSupplierRequest) {
    return this.http.post<SupplierDto>(this.url, req);
  }

  update(id: number, req: UpdateSupplierRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
