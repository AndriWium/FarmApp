# 03 — Architecture

Built around your existing stack: **.NET (C#) API + Angular/TypeScript frontend + MSSQL**. No new languages to learn; the interesting decisions are deployment shape and offline strategy.

## Recommended shape

**DECISION (2026-07): v1 frontend is a desktop-browser website only.** No mobile app and no phone-optimised screens yet — but the API is designed *API-first* so that a future mobile app (or second website) can consume it without backend changes.

```
┌────────────────────────────────────────────────┐
│ Angular web app (desktop browser, v1)          │
│  - Admin/office screens                        │
│  - POS screen (usable on a laptop at a stall)  │
│  - Field/harvest capture (desktop entry, v1)   │
│  [future: mobile app / phone screens → same API]│
└───────────────┬────────────────────────────────┘
                │ REST (JSON), JWT auth, versioned
┌───────────────▼────────────────────────────────┐
│ ASP.NET Core Web API (modular monolith)        │
│  Modules: Farming | Stock | Sales | Reporting  │
│  Repository pattern + EF Core (code-first)     │
│  Background jobs: cost recalc, batch aging     │
└───────────────┬────────────────────────────────┘
┌───────────────▼────────────────────────────────┐
│ SQL Server — LocalDB for dev (doc 07),         │
│  Express/cloud for production                  │
└────────────────────────────────────────────────┘
```

- **Modular monolith, one solution, one database.** Microservices would be pure overhead here. Keep module boundaries as folders/projects so the code stays navigable.
- **One Angular app** with the different areas as lazy-loaded routes — not multiple apps. Shared models/services, different layouts. (See doc 08 for why a future mobile capability still doesn't mean a second application.)

### API-first rules (so a future mobile app "just works")
- All business logic lives behind the API; the Angular app is a pure client. Nothing the website can do is website-only.
- **JWT bearer auth, no cookies/sessions** — token auth works identically from a browser, a phone app, or Postman.
- **Version the API from day one** (`/api/v1/...`). A future mobile app can then evolve against v2 while the site stays on v1.
- DTOs, not entities, over the wire; consistent envelope for errors (ProblemDetails); pagination conventions fixed early.
- CORS configured per-client-origin; OpenAPI/Swagger generated always — it becomes the mobile team's (future you's) contract.
- Design endpoints around *use cases* (e.g. `POST /api/v1/harvests`), not around screens.

## Hosting decision (make this early)

| Option | Pros | Cons |
|--------|------|------|
| **Cloud VM / app service + SQL** (recommended) | Access from anywhere (market, field), backups easier, no farm hardware to die | Monthly cost; internet dependency → mitigated by PWA offline |
| Local PC/server on the farm | No monthly cost, LAN speed | You become the sysadmin; power cuts; no remote access without extra work; backup discipline is on you |

Recommendation: small cloud instance + SQL (or Azure SQL basic tier). The POS offline layer (below) removes the "internet is down" objection.

## Offline strategy

Full design and discussion moved to **[08-offline-sync.md](08-offline-sync.md)**. Summary of the decision: one Angular application (installable as a PWA later — not a second app), an outbox/queue of client-GUID-stamped transactions in the browser's IndexedDB, idempotent upsert endpoints on the API, master data synced one-way server→client on reconnect. For v1 (desktop website) we build the API idempotency + ClientGuid columns now (cheap) and the browser-side queue only when POS-at-the-market becomes real.

## Cross-cutting decisions

- **Auth:** JWT + rotating refresh tokens, roles (Owner, Cashier, Worker) mapped to named policies, deny-by-default. Full design in [13-auth-and-logging.md](13-auth-and-logging.md), which also specifies the middleware-only request/response logging pipeline.
- **Multi-tenancy: no.** Build single-tenant. If it ever becomes a product for other farmers, that's a rewrite decision for later — don't pay the complexity tax now. (Cheap insurance: keep a `TenantId` off, but avoid global state/singletons that assume one farm.)
- **Data access:** **EF Core code-first** (migrations in source control) behind the **repository pattern + unit of work** — see [11-coding-standards.md](11-coding-standards.md) for the exact conventions and SOLID guidelines.
- **Reporting:** EF for transactions; SQL views/stored procs for reports (month-end aggregation in LINQ gets painful and slow). Consider a `Reporting` schema of views so report SQL is versioned and testable.
- **Background jobs:** in-process hosted service (or Hangfire) for: season cost recalculation, best-before/aging flags, debtor aging, backup verification ping. No message bus needed.
- **Attachments** (photos of receipts/invoices, spray records): file storage (blob/container or disk) with path in DB — not varbinary in the database.
- **Audit:** interceptor in EF Core writing to AuditLog for updates/deletes on transactional tables.
- **Backups:** automated daily SQL backup + off-site copy, and a *tested restore*, before go-live. This is farm-business-critical data — a season of records is irreplaceable.
- **Dates/timezone:** store dates as `date` where possible; the business runs in one timezone (SAST) — don't UTC-convert date-only farming facts (a harvest on the 3rd must never report as the 2nd).
- **Money:** decimal end-to-end; watch Angular `number` → API serialization; round only at display and at document totals (define the rounding rule once).

## Suggested solution layout

```
FarmApp.sln
  src/
    FarmApp.Api/            -- ASP.NET Core, controllers per module
    FarmApp.Domain/         -- entities, domain services (costing lives here)
    FarmApp.Infrastructure/ -- EF Core, migrations, report views
    FarmApp.Jobs/           -- hosted services (can live in Api project initially)
  web/
    farm-app/               -- Angular workspace (single app, lazy-loaded modules:
                            --   admin, pos, field, reports)
  db/
    views/                  -- report SQL, versioned
  docs/                     -- these planning files move here
```
