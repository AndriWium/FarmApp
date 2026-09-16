import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  CreateExpenseCategoryRequest,
  ExpenseCategoryDto,
  UpdateExpenseCategoryRequest,
} from './expense-category.model';

@Injectable({ providedIn: 'root' })
export class ExpenseCategoriesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/expense-categories`;

  getAll(includeInactive = false) {
    return this.http.get<ExpenseCategoryDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<ExpenseCategoryDto>(`${this.url}/${id}`);
  }

  create(req: CreateExpenseCategoryRequest) {
    return this.http.post<ExpenseCategoryDto>(this.url, req);
  }

  update(id: number, req: UpdateExpenseCategoryRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
