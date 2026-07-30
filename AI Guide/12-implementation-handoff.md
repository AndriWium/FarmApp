# 12 — Implementation Handoff Brief

**Audience: the agent/developer who implements this plan.** This folder is the complete specification; the planning conversation is not required. Read this file first, then the docs in the order below.

## Read order

1. This file — context and where to start.
2. [README.md](README.md) — one-paragraph summary + confirmed decisions list.
3. [06-roadmap.md](06-roadmap.md) — build phases; you are starting **Phase 0**.
4. [11-coding-standards.md](11-coding-standards.md) — solution layout, SOLID rules, repository pattern, EF code-first conventions. **Binding, not advisory.**
5. [02-data-model.md](02-data-model.md) + [03-architecture.md](03-architecture.md) — what to build.
6. [13-auth-and-logging.md](13-auth-and-logging.md) — Phase 0 builds both of these; read before scaffolding.
7. The rest (01, 04, 05, 07–10, 14) as reference when the relevant phase arrives.

## Non-negotiable decisions (all confirmed by the owner, 2026-07)

| Area | Decision |
|------|----------|
| Frontend v1 | Desktop-browser Angular website only. No mobile screens yet. |
| API | API-first: versioned REST (`/api/v1/...`), JWT bearer (no cookies), Swagger always generated, all business logic behind the API — a future mobile app must be able to consume it unchanged. |
| Payments | **Card-only** (+ EFT, on-account). No cash handling anywhere. Day close = system card total vs card machine batch total. |
| Units | Base unit **kg** (or `each`); pack sizes carry conversion factors. Quantities `decimal(18,3)`, money `decimal(18,2)`. |
| Stock | Append-only `StockMovement` rows; balances always derived, never a mutable on-hand column as source of truth. FIFO batch depletion. |
| Offline | One app, not two. **Build now:** `ClientGuid UNIQUE` on transactional tables + idempotent POST endpoints (replay returns existing resource). **Defer:** service worker / IndexedDB outbox until market-day POS is real. Spec: [08-offline-sync.md](08-offline-sync.md). |
| Costing | Own-grown: season estimate per kg during the season, season-end true-up posted as a labelled, user-approved adjustment. Bought-in: weighted average. Snapshot `UnitCost`/`CostAtSale` on rows; history never recalculates. Spec: [09-costing-explained.md](09-costing-explained.md). |
| Periods | `AccountingPeriod` table + write-path date check from Phase 0 (closed months reject writes). Spec: [10-go-live-controls.md](10-go-live-controls.md). |
| VAT | Not registered; capture `VatAmount` on expenses/purchases from day one anyway. |
| Backend | .NET modular monolith: `Domain` / `Infrastructure` / `Api` projects. Repository interfaces in Domain, EF Core **code-first** implementations in Infrastructure, unit of work = DbContext. Reports read SQL views directly (Dapper/raw), bypassing repositories. |
| Product range | Produce **plus prepared goods** (recipes; made-to-order consumes ingredients at sale, made-in-batch via ProductionBatch) **plus resale merchandise**. Grade is nullable. Docs 01/02. |
| Auth | Login screen; JWT (~15 min) + rotating server-side refresh tokens; `PasswordHasher<T>`; roles Owner/Cashier/Worker → **named policies**, deny-by-default global fallback policy; only login/refresh anonymous. Spec: [13-auth-and-logging.md](13-auth-and-logging.md). |
| Logging | Every request+response logged **only** via middleware (correlation id, user, bodies with redaction/size caps, duration, status) + Serilog structured file sink. **Zero logger calls in services/repositories** — this is a hard rule. Spec: doc 13. |
| Up & Coming tab | In-app roadmap tab (`RoadmapItem` table, Owner-edited, all roles view), seeded per [14-up-and-coming.md](14-up-and-coming.md). |
| Scope guards | Not an accounting package, not payroll, not a webshop, not multi-tenant SaaS. |

## Environment facts (verified 2026-07-26 on this machine)

- **Dev database:** SQL Server 2019 Express **LocalDB**, instance `(localdb)\MSSQLLocalDB`, Windows auth, working. Planned DB name `FarmApp` — let the first EF migration create it.
  `Server=(localdb)\MSSQLLocalDB;Database=FarmApp;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True`
- `sqlcmd` available; Docker Desktop installed but engine not running; nothing on port 1433; no full SQL Server service. Details/production options: [07-database-setup.md](07-database-setup.md).
- Working drive: `H:\`. Create the code repo as a **sibling folder**, e.g. `H:\Studies and Applications\2026-07 Farmers application\src-repo\` or a cleaner path like `H:\FarmApp\` (ask the owner which; docs then move/copy into its `docs/`).
- Check installed SDK versions before scaffolding (`dotnet --list-sdks`, `node -v`, `ng version`); use the latest installed .NET LTS+ and Angular CLI, don't assume.

## Phase 0 concrete task list (from doc 06, expanded)

1. `git init`; solution + projects per doc 11 layout; `.editorconfig`; `.gitignore`; `DECISIONS.md` seeded from README's decision list.
2. EF Core: `FarmAppDbContext`, entity configurations (`IEntityTypeConfiguration<T>`), global decimal/enum-as-string conventions, initial migration, idempotent reference-data seeder (grades, activity types, movement types, expense categories).
3. Auth per doc 13: login + refresh endpoints, `PasswordHasher<T>`, `AppUser` + refresh-token table, named policies + deny-by-default fallback, Angular login screen/interceptor/guards.
3b. Logging per doc 13: CorrelationId, RequestResponseLogging, and ExceptionHandling middleware + Serilog config in `Program.cs` — before any feature endpoints exist, so everything inherits it.
4. `AccountingPeriod` + the write-path period check (EF SaveChanges interceptor or domain service — must be one central enforcement point).
5. Audit interceptor (updates/deletes on transactional tables → `AuditLog`).
6. Angular workspace: standalone components, strict TS, ESLint+Prettier, lazy routes (`admin`, `stock`, `pos`, `farming`, `reports`), typed API layer (consider OpenAPI generation).
7. Master data CRUD (API + screens): products, grades, pack sizes, blocks, crops/cultivars, input items, suppliers, customers, price lists.
8. Opening-balance wizard skeleton (doc 10 §3).
9. Backup script for dev DB once real data exists.
10. "Up & Coming" tab: `RoadmapItem` table + read-only tab + Owner CRUD, seeded from doc 14 (small, do it early — it's where the owner tracks the rest of the build).
11. `Start-FarmApp.ps1` / `Stop-FarmApp.ps1` launcher v1 per doc 14 §1 (LocalDB start → API start → health wait → open browser) + desktop shortcut.

**Phase 0 exit test:** real farm master data captured through the UI; backup taken and restored once.

## Working agreements for the implementing agent

- The owner (Andri) is a .NET/Angular/MSSQL developer — communicate in those terms, and prefer idiomatic stack solutions over clever ones.
- When reality contradicts these docs, don't silently deviate: flag it, propose the change, and update the relevant doc + `DECISIONS.md` in the same piece of work.
- Unit-test the dangerous logic before UI: costing/true-up, FIFO depletion, withholding-period locks, period-close rejection, ClientGuid idempotency.
- Ship each phase usable (capture screens *and* their reports), per doc 06's standing rules.
