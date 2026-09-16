import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreatePurchaseRequest, ProducePurchaseDto } from './produce-purchase.model';

@Injectable({ providedIn: 'root' })
export class ProducePurchasesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/produce-purchases`;

  getAll() {
    return this.http.get<ProducePurchaseDto[]>(this.url);
  }

  getById(id: number) {
    return this.http.get<ProducePurchaseDto>(`${this.url}/${id}`);
  }

  // Creates the header + every line + every resulting StockBatch (+ seeding PurchaseIn movement)
  // atomically in one backend call (task brief) - there's no separate "add line" endpoint.
  create(req: CreatePurchaseRequest) {
    return this.http.post<ProducePurchaseDto>(this.url, req);
  }
}
