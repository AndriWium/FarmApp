# Frontend Guide — Step by Step (Angular 19)

Builds milestone M4 and the FE half of M5, then the patterns every later screen repeats. You have Angular CLI 19.0.2 — the commands below match it.

**Same rule as the backend guide:** every new file states its exact folder path and shows the **complete** file contents. Every edit to an existing file shows the surrounding lines. VS Code's integrated terminal (Ctrl + `` ` ``) is the natural place to run the `ng` commands below — it opens already pointed at whichever folder you have open.

---

## Step 1 — Create the workspace (M4 begins)

In a terminal, standing in `H:\FarmApp`:

```powershell
mkdir web
cd web
ng new farm-app --style=scss --routing --strict
cd farm-app
ng serve
```

Angular CLI will ask a couple of questions — answer:
- **SSR / prerendering?** → **No** (this is an internal app; server-side rendering is pointless complexity here).

(Angular 19 apps are **standalone** by default — no NgModules; components declare their own imports.)

Open `http://localhost:4200` in your browser.

Now open `web\farm-app\src\app\app.component.html` and replace its entire contents with just:
```html
<router-outlet />
```

✅ **Checkpoint:** the browser shows a blank page (nothing routed yet — that's expected) with no red errors in the console (F12). Commit: `"M4.1 angular workspace"`.

## Step 2 — Folder structure + lint

From `H:\FarmApp\web\farm-app`, create the feature folders now (empty folders don't survive in git, but the first file you put in each will):

```powershell
mkdir src\app\core\auth
mkdir src\app\core\layout
mkdir src\app\features\grades
mkdir src\app\shared
```

This gives:
```
core/        auth/, layout/     ← singletons: services, interceptors, guards, the app shell
features/    grades/  (later: products/, stock/, pos/, farming/, reports/, roadmap/)
shared/      reusable dumb components, pipes
```

Add lint + formatting:
```powershell
ng add @angular-eslint/schematics
npm i -D prettier
```

Create `.prettierrc` at `H:\FarmApp\web\farm-app\.prettierrc`:
```json
{
  "singleQuote": true,
  "printWidth": 100
}
```

Open `package.json`, find the `"scripts"` block, and add two entries (comma-separate from the existing ones):
```json
"lint": "ng lint",
"format": "prettier --write \"src/**/*.{ts,html,scss}\""
```

Run both:
```powershell
npm run lint
npm run format
```

✅ **Checkpoint:** `npm run lint` passes on the empty skeleton.

## Step 3 — Talk to the API: config, HttpClient, CORS

*Concept:* the FE never invents URLs all over the place — one base URL from environment config, one service layer that owns HTTP.

### 3.1 — Environment config

**VS Code:** open `src\environments\environment.development.ts` (create the `environments` folder + file if `ng new` didn't generate it — recent CLI versions don't by default):
```ts
export const environment = {
  apiUrl: 'https://localhost:7137/api/v1',   // match your API's actual HTTPS port from `dotnet run`
};
```

### 3.2 — Wire HttpClient

Open `src\app\app.config.ts` — it should currently look roughly like this from `ng new`; add the `provideHttpClient` line:
```ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([])),   // interceptors array fills in at step 6
  ],
};
```

### 3.3 — CORS (on the backend)

The browser blocks `localhost:4200` → `localhost:7137` unless the **API** explicitly permits it. **VS (backend solution):** open `Program.cs` in **FarmApp.Api**, and add, right after `var builder = WebApplication.CreateBuilder(args);`:
```csharp
builder.Services.AddCors(o => o.AddPolicy("Fe",
    p => p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
```
Then in the pipeline section, add `app.UseCors("Fe");` immediately **before** `app.UseAuthentication();` (or before `app.UseAuthorization();` if you haven't built auth yet).

*Burn this into memory:* a CORS error in the browser console means "the backend didn't say I'm allowed" — it is fixed in the **backend**, and the API itself is usually fine (Swagger still works even when the browser complains).

## Step 4 — Typed model + API service for grades

*Concept ([doc 11](<../AI Guide/11-coding-standards.md>) rule):* components never touch `HttpClient`; a per-feature service does. Models are TS interfaces mirroring the API DTOs.

**VS Code:** create the file `src\app\features\grades\grade.model.ts`:
```ts
export interface Grade {
  gradeId: number;
  name: string;
}

export interface CreateGradeRequest {
  name: string;
}
```

Create `src\app\features\grades\grades-api.service.ts`:
```ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CreateGradeRequest, Grade } from './grade.model';

@Injectable({ providedIn: 'root' })
export class GradesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/grades`;

  getAll() {
    return this.http.get<Grade[]>(this.url);
  }

  create(req: CreateGradeRequest) {
    return this.http.post<Grade>(this.url, req);
  }

  update(id: number, req: CreateGradeRequest) {
    return this.http.put<Grade>(`${this.url}/${id}`, req);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
```

## Step 5 — First screen: grades list + add form (M4 done)

*Concepts, one line each:* a **signal** is a value the template auto-updates from (`grades = signal<Grade[]>([])`, template `@for (g of grades(); ...)`). An **Observable** (what HttpClient returns) is a value that arrives later — `.subscribe(...)` is "when it arrives, do this".

**Terminal**, from `web\farm-app`:
```powershell
ng generate component features/grades/grades-page --skip-tests
```
This creates `src\app\features\grades\grades-page\grades-page.component.ts` and `.html` for you (and a `.scss` you can ignore/delete). Replace their contents:

`grades-page.component.ts`:
```ts
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { GradesApiService } from '../grades-api.service';
import { Grade } from '../grade.model';

@Component({
  selector: 'app-grades-page',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './grades-page.component.html',
})
export class GradesPageComponent implements OnInit {
  private api = inject(GradesApiService);
  private fb = inject(FormBuilder);

  grades = signal<Grade[]>([]);
  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll().subscribe((d) => this.grades.set(d));
  }

  add(): void {
    if (this.form.invalid) return;
    this.api.create(this.form.getRawValue()).subscribe(() => {
      this.form.reset();
      this.load();
    });
  }

  remove(id: number): void {
    if (!confirm('Delete this grade?')) return;
    this.api.delete(id).subscribe(() => this.load());
  }
}
```

`grades-page.component.html`:
```html
<h2>Grades</h2>

<form [formGroup]="form" (ngSubmit)="add()">
  <input formControlName="name" placeholder="Grade name" />
  <button type="submit" [disabled]="form.invalid">Add</button>
  @if (form.controls.name.invalid && form.controls.name.touched) {
    <span class="error">Name is required, max 50 characters.</span>
  }
</form>

<table>
  <thead>
    <tr><th>Name</th><th></th></tr>
  </thead>
  <tbody>
    @for (grade of grades(); track grade.gradeId) {
      <tr>
        <td>{{ grade.name }}</td>
        <td><button (click)="remove(grade.gradeId)">Delete</button></td>
      </tr>
    }
  </tbody>
</table>
```

Route it. Open `src\app\app.routes.ts` and replace its contents:
```ts
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'grades',
    loadComponent: () =>
      import('./features/grades/grades-page/grades-page.component').then((m) => m.GradesPageComponent),
  },
  { path: '', redirectTo: 'grades', pathMatch: 'full' },
];
```

Run both API (F5 in Visual Studio) and FE (`ng serve` in the terminal) together; browse to `http://localhost:4200`, add "Class 1" from the browser, and watch the row land in SSMS. That's the full loop — FE → API → DB — the moment the project becomes real.

*Left as practice, using the exact same pattern as `add()`/`remove()` above:* an **edit** action — add an `editingId` signal, a second small form or inline `<input>` per row, and a `save(id)` method calling `api.update(...)`.

✅ **Checkpoint:** list + add + delete work from the browser without touching Swagger; typing a 51-character name shows a validation message from the FE, and if you temporarily comment out the FE `Validators.maxLength(50)`, the API's 400 response is visible in the Network tab instead. Commit: `"M4 grades screen"`. **This screen is the template for every master-data screen.**

## Step 6 — Login, interceptor, guard (M5 FE half) — spec: [doc 13](<../AI Guide/13-auth-and-logging.md>)

### 6.1 — AuthService

**VS Code:** create `src\app\core\auth\auth.service.ts`:
```ts
import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';

interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}

interface JwtPayload {
  sub: string;
  unique_name: string;
  role: string;
  exp: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private accessToken = signal<string | null>(null);

  currentUser = computed<JwtPayload | null>(() => {
    const token = this.accessToken();
    return token ? this.decode(token) : null;
  });
  isLoggedIn = computed(() => this.currentUser() !== null);

  login(userName: string, password: string) {
    return this.http
      .post<TokenResponse>(`${environment.apiUrl}/auth/login`, { userName, password })
      .pipe(tap((res) => this.setSession(res)));
  }

  refresh() {
    const refreshToken = localStorage.getItem('refreshToken');
    return this.http
      .post<TokenResponse>(`${environment.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(tap((res) => this.setSession(res)));
  }

  logout(): void {
    this.accessToken.set(null);
    localStorage.removeItem('refreshToken');
    this.router.navigateByUrl('/login');
  }

  getAccessToken(): string | null {
    return this.accessToken();
  }

  hasRole(role: string): boolean {
    return this.currentUser()?.role === role;
  }

  private setSession(res: TokenResponse): void {
    this.accessToken.set(res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);
  }

  private decode(token: string): JwtPayload {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
```

### 6.2 — Login page

**Terminal:**
```powershell
ng generate component core/auth/login-page --skip-tests
```

`login-page.component.ts`:
```ts
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login-page.component.html',
})
export class LoginPageComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  errorMessage = '';
  form = this.fb.nonNullable.group({
    userName: ['', Validators.required],
    password: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) return;
    const { userName, password } = this.form.getRawValue();
    this.auth.login(userName, password).subscribe({
      next: () => this.router.navigateByUrl('/grades'),
      error: () => (this.errorMessage = 'Incorrect username or password.'),
    });
  }
}
```

`login-page.component.html`:
```html
<h2>Log in</h2>
<form [formGroup]="form" (ngSubmit)="submit()">
  <input formControlName="userName" placeholder="Username" />
  <input formControlName="password" type="password" placeholder="Password" />
  <button type="submit" [disabled]="form.invalid">Log in</button>
  @if (errorMessage) {
    <p class="error">{{ errorMessage }}</p>
  }
</form>
```

### 6.3 — Auth interceptor (attaches the token, refreshes on 401)

Create `src\app\core\auth\auth.interceptor.ts`:
```ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  if (req.url.includes('/auth/login') || req.url.includes('/auth/refresh')) {
    return next(req);
  }

  const token = auth.getAccessToken();
  const authedReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authedReq).pipe(
    catchError((err) => {
      if (err.status === 401) {
        return auth.refresh().pipe(
          switchMap(() => {
            const retried = req.clone({
              setHeaders: { Authorization: `Bearer ${auth.getAccessToken()}` },
            });
            return next(retried);
          }),
          catchError((refreshErr) => {
            auth.logout();
            return throwError(() => refreshErr);
          }),
        );
      }
      return throwError(() => err);
    }),
  );
};
```
(The retry logic is the fiddliest code in the FE — take the guide's 45-minute rule seriously if this one fights you.)

### 6.4 — Route guard

Create `src\app\core\auth\auth.guard.ts`:
```ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isLoggedIn() ? true : router.parseUrl('/login');
};
```

### 6.5 — Wire it all into app.config.ts and app.routes.ts

Open `src\app\app.config.ts` and add the interceptor to the array from step 3.2:
```ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
  ],
};
```

Open `src\app\app.routes.ts` and replace its contents to add the login route and guard the rest:
```ts
import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./core/auth/login-page/login-page.component').then((m) => m.LoginPageComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: 'grades',
        loadComponent: () =>
          import('./features/grades/grades-page/grades-page.component').then((m) => m.GradesPageComponent),
      },
      { path: '', redirectTo: 'grades', pathMatch: 'full' },
    ],
  },
];
```

✅ **Checkpoint:** opening `/grades` logged-out redirects to `/login`; after login everything works; after 15 idle minutes (token expiry) the next API call silently refreshes — watch it happen in DevTools' Network tab (a `401` on your call, immediately followed by a `refresh` call, then a retry that succeeds). Commit: `"M5 auth FE"`.

## Step 7 — App shell + navigation

**Terminal:**
```powershell
ng generate component core/layout/shell --skip-tests
```

`shell.component.ts` (`src\app\core\layout\shell\shell.component.ts`):
```ts
import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: './shell.component.html',
})
export class ShellComponent {
  auth = inject(AuthService);
}
```

`shell.component.html`:
```html
<nav>
  <a routerLink="/grades">Grades</a>
  <!-- later: Products, Stock, POS, Reports, Up & Coming -->
  <span>{{ auth.currentUser()?.unique_name }}</span>
  <button (click)="auth.logout()">Log out</button>
</nav>

<router-outlet />
```

Now make the shell the parent of your authenticated routes. Open `src\app\app.routes.ts` again and wrap the guarded children in the shell:
```ts
import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { ShellComponent } from './core/layout/shell/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./core/auth/login-page/login-page.component').then((m) => m.LoginPageComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'grades',
        loadComponent: () =>
          import('./features/grades/grades-page/grades-page.component').then((m) => m.GradesPageComponent),
      },
      { path: '', redirectTo: 'grades', pathMatch: 'full' },
    ],
  },
];
```

Styling note: hand-rolled SCSS is fine to start. If you'd rather have ready-made tables/dialogs, `ng add @angular/material` is a pragmatic choice for an internal app — entirely your call, not required by anything above.

The **Up & Coming** tab ([doc 14](<../AI Guide/14-up-and-coming.md>)) is a good solo exercise once you reach it: the simplest feature in the app — build its API (entity → controller, step 2/4/5 of the backend guide) and screen (steps 4–5 here) entirely without hand-holding, and see how far the patterns carry you.

✅ **Checkpoint:** navigating to `/grades` shows the nav bar with a working "Log out" button; logging out redirects to `/login`.

## Step 8 — The repeatable recipe (M7+)

Every screen from here on is the same seven moves — this is the whole job now:

```
model interface (features/<name>/<name>.model.ts)
→ api service (features/<name>/<name>-api.service.ts)
→ list component (features/<name>/<name>-page/, via `ng generate component`)
→ form/edit UI (same component or a second one)
→ lazy route (app.routes.ts, inside the ShellComponent's children)
→ nav link (shell.component.html)
→ role-gate the buttons (`@if (auth.hasRole('Owner')) { ... }`)
```

Guidance for the bigger screens ahead:
- **Master data screens** (M7): pure reps of steps 4–5. Do Products first (ProductType/MakeMode dropdowns make it the most interesting one).
- **POS screen** (Phase 2): big-button product grid, a basket as a `signal` of lines, payment dialog. It's still just components + a service — only the layout is unusual. Design for a laptop at a stall ([doc 01](<../AI Guide/01-scope-and-modules.md>)).
- **Reports** (Phase 4): tables from the Reporting endpoints + every number drillable ([doc 04](<../AI Guide/04-reporting.md>)). Add a chart library only if a real need appears.
- **Keep components dumb**: data-fetching in services, components render + emit. When a component crosses ~200 lines, split it.
- DTO drift (API renamed a field, FE didn't notice) is your most likely recurring bug — when it bites, consider generating TS models from Swagger (`openapi-typescript`) and note it in `LEARNING.md`.

## When something breaks — FE edition

- **F12 first, always.** Console tab: red error text. Network tab: click the failing call — status `0` = API not running/CORS, `401` = token, `403` = role, `400` = look at the response body (your API tells you the field), `500` = go read the API's log file (M6 makes this pleasant — grab the correlation ID from the response headers).
- The error is almost never "Angular is broken". It's a typo'd URL, a missing provider, or a mismatched field name — in that order of likelihood.
