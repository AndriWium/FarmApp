import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  CreateProductRequest,
  ProductDto,
  ProductWithRecipeDto,
  SetRecipeRequest,
  UpdateProductRequest,
} from './product.model';

@Injectable({ providedIn: 'root' })
export class ProductsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/products`;

  getAll(includeInactive = false) {
    return this.http.get<ProductDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<ProductDto>(`${this.url}/${id}`);
  }

  create(req: CreateProductRequest) {
    return this.http.post<ProductDto>(this.url, req);
  }

  update(id: number, req: UpdateProductRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  getRecipe(id: number) {
    return this.http.get<ProductWithRecipeDto>(`${this.url}/${id}/recipe`);
  }

  setRecipe(id: number, req: SetRecipeRequest) {
    return this.http.put<ProductWithRecipeDto>(`${this.url}/${id}/recipe`, req);
  }
}
