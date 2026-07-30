# 11 — Coding Standards & Project Conventions

House rules for keeping the codebase clean. Short on purpose — a standards doc nobody reads is worse than none.

## Solution structure (clean-ish architecture, pragmatic)

```
FarmApp.sln
  src/
    FarmApp.Domain/           -- entities, enums, domain services (costing!),
                              -- repository INTERFACES. No EF, no web, no refs out.
    FarmApp.Infrastructure/   -- EF Core DbContext, migrations, repository
                              -- IMPLEMENTATIONS, file storage, email/etc.
    FarmApp.Api/              -- controllers, DTOs, validation, auth, DI wiring
  tests/
    FarmApp.Domain.Tests/     -- pure unit tests (costing rules live here)
    FarmApp.Api.Tests/        -- integration tests (in-memory/localdb)
  web/farm-app/               -- Angular workspace
  db/views/                   -- report SQL, versioned
  docs/                       -- these documents
```

Dependency rule: `Api → Infrastructure → Domain`. Domain references nothing. Business rules (withholding-period checks, FIFO depletion, costing) live in Domain services — testable without a database or web server.

## SOLID, applied concretely (not as a poster)

- **S — Single responsibility:** controllers translate HTTP ↔ use case, nothing else. A class named `...Manager` or `...Helper` that grows past ~300 lines is a smell — split by use case (`RecordHarvestService`, `CloseTillSessionService`).
- **O — Open/closed:** vary behaviour with strategy interfaces where variation is *known to be coming*: `IPaymentMethodHandler` (card now, maybe cash later), `IReceiptRenderer` (screen now, thermal printer later). Don't pre-abstract things with no second implementation on the horizon.
- **L — Liskov:** keep inheritance rare; prefer composition. Entities are not class hierarchies (no `CashSale : Sale`) — use type discriminator columns/enums instead.
- **I — Interface segregation:** small role interfaces (`IStockDepleter`) over god interfaces (`IStockService` with 30 methods).
- **D — Dependency inversion:** Domain defines interfaces; Infrastructure implements; Api wires up via built-in DI. Constructor injection only; no service locator; no `new`-ing dependencies inside logic classes.

## Repository pattern + EF Core (code-first)

- **Interfaces in Domain**, e.g. `IStockBatchRepository`, `ISaleRepository`; implementations in Infrastructure wrap `FarmAppDbContext`.
- **Generic base for the boring parts** (`IRepository<T>`: GetById, Add, query by spec), **specific repositories for real questions** (`GetOpenBatchesFifoAsync(productId, gradeId)`) — never leak `IQueryable` out of the repository; queries are the repository's job.
- **Unit of Work:** the DbContext *is* the unit of work — expose `IUnitOfWork.SaveChangesAsync()` and let one use-case service call it once per operation, so a sale + its stock movements + payment commit atomically.
- **Reports bypass repositories** deliberately: read-only Dapper/raw SQL against the `Reporting` schema views (doc 04). Repositories are for transactional aggregates, not for aggregation queries — forcing reports through repositories is pattern zealotry.
- **EF code-first conventions:**
  - Migrations in source control, named descriptively (`AddWithholdingPeriodToInputItem`); never edit an applied migration.
  - Fluent configuration in `IEntityTypeConfiguration<T>` classes (one per entity), not data annotations on domain entities.
  - `decimal(18,2)` money / `decimal(18,3)` quantity value conversions set globally; all enums stored as strings for report readability.
  - No lazy loading; explicit `Include` or projection. `AsNoTracking` default for reads.
  - Seed reference data (activity types, grades, movement types) via migration/seeder, idempotent.

## API conventions

- Routes: `/api/v1/{resource}` plural kebab-case; use-case POSTs where CRUD doesn't fit (`POST /api/v1/till-sessions/{id}/close`).
- DTOs per endpoint (request/response records), FluentValidation on requests, AutoMapper optional — hand-mapping is fine and clearer at this size.
- Errors: RFC 7807 `ProblemDetails` everywhere; validation errors 400 with field map; never leak exceptions.
- Idempotency: all transactional POSTs accept `ClientGuid` (doc 08) and return the existing resource on replay.
- Swagger/OpenAPI always on (auth-protected in prod); it is the contract for any future client.

## Angular conventions

- Standalone components, lazy-loaded feature routes: `admin/`, `pos/`, `stock/`, `farming/`, `reports/`.
- Structure per feature: `feature/ {components, services, models}` — models are TS interfaces mirroring API DTOs (consider generating from OpenAPI to stop drift).
- State: keep it boring — services with signals (or RxJS) per feature; no NgRx unless pain proves the need.
- All API access through one generated/typed `ApiService` layer; components never call `HttpClient` directly.
- Strict TypeScript (`strict: true`), ESLint + Prettier enforced, no `any` without an eyebrow-raising comment.

## General hygiene

- Git from day one (repo per solution, conventional-ish commit messages); `main` always buildable.
- `DECISIONS.md` in the repo root — one line per irreversible-ish decision, dated (the docs 01–10 decisions get copied there at scaffold time).
- No secrets in the repo: connection strings via user-secrets (dev) / environment (prod).
- Tests: costing, FIFO depletion, withholding-period, and period-lock rules get unit tests *before* UI exists — they're the rules that silently corrupt data when wrong.
- Formatting: `.editorconfig` committed; `dotnet format` clean.
