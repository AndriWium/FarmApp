import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CurrentUser, JwtPayload, LoginRequest, TokenResponse, toCurrentUser } from './auth.models';

const REFRESH_TOKEN_KEY = 'refreshToken';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  // Access token lives in memory only (never localStorage) - lost on a hard page reload by
  // design (doc 13). The refresh token is the thing persisted, in localStorage.
  private accessToken = signal<string | null>(null);

  currentUser = computed<CurrentUser | null>(() => {
    const token = this.accessToken();
    return token ? toCurrentUser(this.decode(token)) : null;
  });

  isLoggedIn = computed(() => this.currentUser() !== null);

  login(userName: string, password: string) {
    const req: LoginRequest = { userName, password };
    return this.http
      .post<TokenResponse>(`${environment.apiUrl}/auth/login`, req)
      .pipe(tap((res) => this.setSession(res)));
  }

  refresh() {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    return this.http
      .post<TokenResponse>(`${environment.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(tap((res) => this.setSession(res)));
  }

  logout(): void {
    this.accessToken.set(null);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this.router.navigateByUrl('/login');
  }

  getAccessToken(): string | null {
    return this.accessToken();
  }

  /** UI-only convenience gate (e.g. hiding a button). The API's [Authorize] policies are the
   *  real enforcement - see doc 13. */
  hasRole(role: string): boolean {
    return this.currentUser()?.role === role;
  }

  private setSession(res: TokenResponse): void {
    this.accessToken.set(res.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, res.refreshToken);
  }

  private decode(token: string): JwtPayload {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
