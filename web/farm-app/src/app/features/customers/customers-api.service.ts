import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateCustomerRequest, CustomerDto, UpdateCustomerRequest } from './customer.model';

@Injectable({ providedIn: 'root' })
export class CustomersApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/customers`;

  getAll(includeInactive = false) {
    return this.http.get<CustomerDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<CustomerDto>(`${this.url}/${id}`);
  }

  // Personal data (name/phone) always travels in the request body, never in a query string or
  // route parameter - see customer.model.ts's comment.
  create(req: CreateCustomerRequest) {
    return this.http.post<CustomerDto>(this.url, req);
  }

  update(id: number, req: UpdateCustomerRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
