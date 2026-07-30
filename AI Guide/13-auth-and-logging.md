# 13 — Authentication, Authorization & API-Wide Logging

Two cross-cutting API concerns. Both follow the same philosophy: **implemented once in the pipeline, invisible in business code.**

## Part A — Login & authorization

### Authentication (who are you)

- **Login screen** (Angular): username + password → `POST /api/v1/auth/login` → returns **JWT access token** (short-lived, ~15 min) + **refresh token** (long-lived, ~14 days, stored server-side per user/device, revocable).
- Angular stores the access token in memory, refresh token in an httpOnly-style pattern isn't available for a pure SPA + future mobile — so: refresh token in `localStorage`, rotated on every refresh (`POST /api/v1/auth/refresh`), old one invalidated. Acceptable risk profile for this app; rotation + revocation limits damage.
- An Angular **HTTP interceptor** attaches the bearer token and transparently refreshes on 401-expired; a **route guard** redirects unauthenticated users to login.
- **Password storage:** never plain, never home-rolled — use ASP.NET Core's `PasswordHasher<T>` (PBKDF2, per-user salt, versioned format). Full ASP.NET Identity is optional; the hasher + own `AppUser` table is enough at this scale.
- Optional later: short **PIN** re-unlock for fast user switching at the POS (PIN only unlocks an already-authenticated device session; it is not a login).
- Lockout after N failed attempts (log it — see Part B); no self-registration — the Owner creates users.

### Authorization (what may you do)

- **Roles** (from doc 03): `Owner`, `Cashier`, `Worker`. Role claims inside the JWT.
- Enforced **at the API**, never only in the UI — hiding a button is UX, not security. Every controller/endpoint carries an explicit `[Authorize(Policy = ...)]`; there is **no anonymous endpoint except login/refresh** (deny-by-default via a global fallback policy).
- Use **policy names that say what, not who** — `CanSell`, `CanManageStock`, `CanCloseperiod`, `CanManageUsers`, `CanViewReports` — mapped to roles in one place (`AuthorizationPolicies.cs`). Adding a role later means editing one file, not every controller (SOLID: open/closed).

| Policy | Owner | Cashier | Worker |
|---------------------|-------|---------|--------|
| CanSell (POS) | ✔ | ✔ | |
| CanCaptureFarming | ✔ | | ✔ |
| CanManageStock | ✔ | ✔ (movements) | ✔ (harvest/wastage) |
| CanManageMasterData | ✔ | | |
| CanViewReports | ✔ | | |
| CanClosePeriod / approve true-up | ✔ | | |
| CanManageUsers | ✔ | | |

- The Angular side mirrors policies with a `*hasPolicy` structural directive / guard fed from the token claims — purely cosmetic gating; the API remains the enforcement point.
- Audit tie-in: the authenticated `UserId` flows into `AuditLog` and every transactional row's `CreatedBy` automatically (see Part B's middleware — same enrichment).

## Part B — Logging every call, with zero `logger.Add()` in business code

**Requirement:** log every API call and its response before it returns to the FE — *without* sprinkling logging statements through services and repositories.

### How: middleware + pipeline hooks, not manual calls

```
Request ──▶ CorrelationIdMiddleware        (new GUID per request, returned in
        │                                   X-Correlation-Id header)
        ├─▶ RequestResponseLoggingMiddleware
        │     • captures: timestamp, user, method, path, query,
        │       request body, status code, response body, duration ms
        │     • one structured log event per request, written AFTER the
        │       response is produced, just before it goes to the FE
        ├─▶ ExceptionHandlingMiddleware     (unhandled → 500 ProblemDetails,
        │                                   logged with same correlation id)
        └─▶ Controllers / services / repos  ← contain NO logging code at all
```

- **Serilog** as the logging framework, configured in `Program.cs` only:
  - Structured JSON events (queryable), enriched automatically with `CorrelationId`, `UserId`, `UserName`, machine, environment.
  - Sinks: rolling file (`logs/farmapp-YYYYMMDD.json`, e.g. 31-day retention) + console in dev. A viewer like **Seq** (free single-user) is a nice dev add-on — browse/filter logs in a UI instead of files.
- **Response body capture** works by swapping `HttpContext.Response.Body` for a `MemoryStream` in the middleware, logging its content, then copying to the real stream — standard pattern, implemented once.
- **EF Core SQL logging**: dev-only via Serilog category filter (`Microsoft.EntityFrameworkCore.Database.Command` at Information in dev, Warning in prod) — again zero code in repositories.
- If per-domain-event logging is ever wanted (e.g. "period closed"), prefer the **audit log table** (doc 02) over scattered log statements — business events belong in the DB, diagnostics belong in Serilog.

### Guardrails (decide now, they're annoying later)

- **Redaction:** never log passwords/PINs/tokens — the middleware redacts known-sensitive fields (`password`, `pin`, `refreshToken`, `Authorization` header) by property-name filter before writing.
- **Size limits:** truncate logged bodies at e.g. 8 KB (report exports and photo uploads would otherwise bloat logs); log `[truncated, full size N]`.
- **Exclusions:** skip body capture for swagger, health checks, static files, and binary content types.
- **Levels:** 2xx/3xx → Information; 4xx → Warning; 5xx/exceptions → Error. Failed logins and lockouts logged explicitly as Warning by the auth service (the one deliberate exception to "no manual logging", or capture via a 401-status rule in middleware).
- **Retention/PII:** logs contain customer names in payloads — treat log files with the same care as the DB (backup exclusion is fine, access is not public, retention capped).

### Why this satisfies SOLID
Logging is a cross-cutting concern; middleware keeps it in one class with one responsibility. Services and repositories stay pure — they can be unit-tested without a logger, and the logging behaviour can change (new sink, new redaction rule) without touching a single business file.

## Build phasing

- Phase 0: auth (login/refresh/hasher/roles/fallback policy), correlation + request/response logging middleware, exception middleware, Serilog config. These are foundations — everything built afterwards inherits them for free.
- Policy matrix grows with each phase's endpoints; the table above is the seed.
