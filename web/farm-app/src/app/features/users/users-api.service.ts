import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  CreateUserRequest,
  ResetPasswordRequest,
  SetUserActiveRequest,
  UpdateUserRoleRequest,
  UserDto,
} from './user.model';

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/users`;

  getAll(includeInactive = false) {
    return this.http.get<UserDto[]>(this.url, { params: { includeInactive } });
  }

  create(req: CreateUserRequest) {
    return this.http.post<UserDto>(this.url, req);
  }

  setActive(id: number, req: SetUserActiveRequest) {
    return this.http.put<void>(`${this.url}/${id}/active`, req);
  }

  updateRole(id: number, req: UpdateUserRoleRequest) {
    return this.http.put<void>(`${this.url}/${id}/role`, req);
  }

  resetPassword(id: number, req: ResetPasswordRequest) {
    return this.http.post<void>(`${this.url}/${id}/reset-password`, req);
  }
}
