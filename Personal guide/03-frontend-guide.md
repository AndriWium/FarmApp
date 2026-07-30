# Frontend Guide — Step by Step (Angular 19)

Builds milestones M4 and the FE half of M5, then the patterns every later screen repeats. You have Angular CLI 19.0.2 — the commands below match it. Type, don't paste.

---

## Step 1 — Create the workspace (M4 begins)

```powershell
cd H:\FarmApp\web
ng new farm-app --style=scss --routing --strict
cd farm-app
ng serve
```

(Angular 19 apps are **standalone** by default — no NgModules; components declare their own imports. If asked about SSR/prerendering, say **No** — this is an internal app, SSR is pointless complexity here.)

Open `http://localhost:4200`.

✅ **Checkpoint:** the Angular placeholder page loads. Delete the placeholder content from `app.component.html`, leave `<router-outlet />`. Commit: `"M4.1 angular workspace"`.

## Step 2 — Folder structure + lint (10 min of discipline that pays forever)

Under `src/app/`, create the shape from [doc 11](<../AI Guide/11-coding-standards.md>):

```
core/        auth/, api/, layout/     ← singletons: services, interceptors, shell
features/    grades/  (later: products/, stock/, pos/, farming/, reports/, roadmap/)
shared/      reusable dumb components, pipes
```

Add lint/format: `ng add @angular-eslint/schematics`, then `npm i -D prettier` + a `.prettierrc`. Add scripts `"lint"` and `"format"` to package.json and run both.

✅ **Checkpoint:** `npm run lint` passes on the empty skeleton.

## Step 3 — Talk to the API: config, HttpClient, CORS

*Concept:* the FE never invents URLs all over the place — one base URL from environment config, one service layer that owns HTTP.

1. `src/environments/environment.development.ts`: `apiUrl: 'http://localhost:5000/api/v1'` (use your API's actual port from `dotnet run`).
2. In `app.config.ts` providers: `provideHttpClient(withInterceptors([]))` — the empty array gets filled in step 6.
3. **CORS — meet it on your terms.** The browser blocks `localhost:4200` → `localhost:5000` unless the *API* permits it. In `Program.cs` (backend):
```csharp
builder.Services.AddCors(o => o.AddPolicy("Fe",
    p => p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
app.UseCors("Fe");   // before UseAuthentication
```
*Burn this into memory:* a CORS error in the browser console means "the backend didn't say I'm allowed" — it is fixed in the **backend**, and the API itself is usually fine (Swagger still works).

## Step 4 — Typed model + API service for grades

*Concept ([doc 11](<../AI Guide/11-coding-standards.md>) rule):* components never touch `HttpClient`; a per-feature service does. Models are TS interfaces mirroring the API DTOs.

`features/grades/grade.model.ts`:
```ts
export interface Grade { gradeId: number; name: string; }        // note: camelCase — JSON serializer does this
export interface CreateGradeRequest { name: string; }
```
`features/grades/grades-api.service.ts`:
```ts
@Injectable({ providedIn: 'root' })
export class GradesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/grades`;

  getAll()                        { return this.http.get<Grade[]>(this.url); }
  create(req: CreateGradeRequest) { return this.http.post<Grade>(this.url, req); }
  update(id: number, req: CreateGradeRequest) { return this.http.put<Grade>(`${this.url}/${id}`, req); }
  delete(id: number)              { return this.http.delete<void>(`${this.url}/${id}`); }
}
```

## Step 5 — First screen: grades list + add form (M4 done)

*Concepts, one line each:* a **signal** is a value the template auto-updates from (`grades = signal<Grade[]>([])`, template `@for (g of grades(); ...)`). An **Observable** (what HttpClient returns) is a value that arrives later — `.subscribe(...)` is "when it arrives, do this".

1. `ng g c features/grades/grades-page` — inject `GradesApiService`; in `ngOnInit`, `getAll().subscribe(d => this.grades.set(d))`.
2. Template: a simple table with `@for`, an "Add" form. Use a **reactive form** (`FormBuilder`, `Validators.required, maxLength(50)`) — the FE mirror of your FluentValidation rules. On submit → `create(...)` → on success, reload the list and `form.reset()`.
3. Route it in `app.routes.ts` lazily:
```ts
{ path: 'grades', loadComponent: () => import('./features/grades/grades-page/grades-page.component')
    .then(m => m.GradesPageComponent) },
```
4. Run API + FE together; add "Class 1" from the browser and watch the row land in SSMS. That's the full loop — FE → API → DB — the moment the project becomes real.
5. Add edit (small form per row or reuse the form) and delete (with `confirm()` for now).

✅ **Checkpoint:** full CRUD from the browser without touching Swagger; a 44-character name shows a validation message from *both* sides (FE blocks it; and if you disable the FE check, the API's 400 arrives). Commit: `"M4 grades screen"`. **This screen is the template for every master-data screen.**

## Step 6 — Login, interceptor, guard (M5 FE half) — spec: [doc 13](<../AI Guide/13-auth-and-logging.md>)

Build in this order, testing each piece:

1. **AuthService** (`core/auth/`): signals `currentUser` + `isLoggedIn` (computed); `login()` posts to `/auth/login`, keeps the access token in a private field (memory), refresh token in `localStorage`; `logout()` clears both; decode the JWT payload with `JSON.parse(atob(token.split('.')[1]))` for name/role claims.
2. **Login page** (`ng g c core/auth/login-page`): username + password reactive form → `AuthService.login()` → navigate to `/grades`; wrong password shows a friendly message (map the 401).
3. **Auth interceptor** (functional, in the `withInterceptors([...])` array): clone every request with `Authorization: Bearer <token>`; skip for `/auth/` URLs. On a 401 response: call refresh once, retry the original request; if refresh also fails → `logout()` + redirect to login. (The retry logic is the fiddliest code in the FE — take the 45-minute rule seriously here.)
4. **Route guard** (`CanActivateFn`): not logged in → `router.parseUrl('/login')`. Apply to every route except `/login`.
5. **Role-aware UI**: a tiny `hasRole(role: string)` on AuthService; hide the "delete" button from non-Owners with `@if`. Say it out loud while doing it: *hiding buttons is politeness; the API's 403 is the security* ([doc 13](<../AI Guide/13-auth-and-logging.md>)).

✅ **Checkpoint:** opening `/grades` logged-out lands on login; after login everything works; after 15 idle minutes (token expiry) the next call silently refreshes — watch it happen in DevTools' Network tab. Commit: `"M5 auth FE"`.

## Step 7 — App shell + navigation

`core/layout/shell.component`: sidebar/topbar with links (Grades, later Products, Stock, POS, Reports, **Up & Coming**, logged-in-as + logout), `<router-outlet />` for content; make it the parent of all authenticated routes (children array). Keep styling basic — either hand-rolled SCSS or Angular Material (`ng add @angular/material`) if you want ready-made tables/dialogs; Material is the pragmatic choice for an internal app, but it's your call. The **Up & Coming** tab ([doc 14](<../AI Guide/14-up-and-coming.md>)) is a perfect solo exercise: simplest feature in the app — build its API + screen entirely without the guide and see how far the patterns carry you.

✅ **Checkpoint:** navigation between two working screens inside one layout.

## Step 8 — The repeatable recipe (M7+)

Every screen from here on is the same seven moves — this is the whole job now:

```
model interface → feature api service → list component → form component
→ lazy route → nav link → role-gate the buttons
```

Guidance for the bigger screens ahead:
- **Master data screens** (M7): pure reps of steps 4–5. Do Products first (ProductType/MakeMode dropdowns make it the most interesting one).
- **POS screen** (Phase 2): big-button product grid, a basket as a signal of lines, payment dialog. It's still just components + a service — only the layout is unusual. Design for a laptop at a stall ([doc 01](<../AI Guide/01-scope-and-modules.md>)).
- **Reports** (Phase 4): tables from the Reporting endpoints + every number drillable ([doc 04](<../AI Guide/04-reporting.md>)). Add a chart lib only if a real need appears.
- **Keep components dumb**: data-fetching in services, components render + emit. When a component crosses ~200 lines, split it.
- DTO drift (API renamed a field, FE didn't notice) is your most likely recurring bug — when it bites, consider generating TS models from Swagger (`openapi-typescript`) and note it in LEARNING.md.

## When something breaks — FE edition

- **F12 first, always.** Console tab: red error text. Network tab: click the failing call — status 0 = API not running/CORS, 401 = token, 403 = role, 400 = look at the response body (your API tells you the field), 500 = go read the API's log file (M6 makes this pleasant — grab the correlation id from the response headers).
- The error is almost never "Angular is broken". It's a typo'd URL, a missing provider, or a mismatched field name — in that order of likelihood.
