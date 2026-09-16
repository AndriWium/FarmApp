import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateInputPurchaseRequest, InputPurchaseDto } from './input-purchase.model';

@Injectable({ providedIn: 'root' })
export class InputPurchasesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/input-purchases`;

  getAll() {
    return this.http.get<InputPurchaseDto[]>(this.url);
  }

  getById(id: number) {
    return this.http.get<InputPurchaseDto>(`${this.url}/${id}`);
  }

  // Creates the header + every line + every resulting PurchaseIn InputStockMovement atomically in
  // one backend call (InputPurchaseService.CreatePurchaseAsync) - same shape as
  // ProducePurchasesApiService.create, no separate "add line" endpoint.
  create(req: CreateInputPurchaseRequest) {
    return this.http.post<InputPurchaseDto>(this.url, req);
  }
}
