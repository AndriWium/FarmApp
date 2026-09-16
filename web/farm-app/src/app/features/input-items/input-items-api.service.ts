import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateInputItemRequest, InputItemDto, UpdateInputItemRequest } from './input-item.model';

@Injectable({ providedIn: 'root' })
export class InputItemsApiService {
  private http = inject(HttpClient);
  // NOT kebab-case: InputItemsController left [Route("api/v1/[controller]")] on the default
  // [controller] token instead of an explicit "input-items" route (unlike its sibling
  // controllers - TillSessions, ProducePurchases, etc. - which all override it). The real,
  // verified route is "api/v1/InputItems" (case-insensitive per ASP.NET routing defaults).
  // Backend is out of scope for this phase per the task brief ("never touch... unless blocking");
  // this isn't blocking, so the frontend just points at the real path. Logged in DECISIONS.md.
  private url = `${environment.apiUrl}/InputItems`;

  getAll(includeInactive = false) {
    return this.http.get<InputItemDto[]>(this.url, { params: { includeInactive } });
  }

  getById(id: number) {
    return this.http.get<InputItemDto>(`${this.url}/${id}`);
  }

  create(req: CreateInputItemRequest) {
    return this.http.post<InputItemDto>(this.url, req);
  }

  update(id: number, req: UpdateInputItemRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  deactivate(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
