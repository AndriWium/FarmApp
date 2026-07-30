# Farmers Application — Planning Documents

> **Building it yourself?** Start with [guide/00-how-to-use-this-guide.md](guide/00-how-to-use-this-guide.md) — a step-by-step self-build curriculum (SQL / backend / frontend) based on these plans. The planning docs below are frozen; changes only by explicit decision.

Planning for a small-scale farming + fruit selling management application.
Owner: Andri (farmer/fruit seller; .NET, Angular/TS, MSSQL developer).

## Documents

| File | Contents |
|------|----------|
| [01-scope-and-modules.md](01-scope-and-modules.md) | The five core areas broken into concrete features |
| [02-data-model.md](02-data-model.md) | Draft entity/table design for MSSQL |
| [03-architecture.md](03-architecture.md) | Tech stack decisions (.NET + Angular + MSSQL), offline strategy |
| [04-reporting.md](04-reporting.md) | Month-end and operational reports, with the data each one needs |
| [05-things-you-havent-thought-of.md](05-things-you-havent-thought-of.md) | Gotchas and blind spots — read this one first |
| [06-roadmap.md](06-roadmap.md) | Phased build order (MVP → full system) |
| [07-database-setup.md](07-database-setup.md) | SQL scan result: LocalDB found + connection details; production options |
| [08-offline-sync.md](08-offline-sync.md) | How offline sync works (outbox + ClientGuid); why it's still one app |
| [09-costing-explained.md](09-costing-explained.md) | Own-grown costing explained with a worked example |
| [10-go-live-controls.md](10-go-live-controls.md) | Month-end locking, VAT readiness, opening balances, backups |
| [11-coding-standards.md](11-coding-standards.md) | SOLID, repository pattern, EF code-first, Angular conventions |
| [12-implementation-handoff.md](12-implementation-handoff.md) | **Start here if you are the implementing agent** — brief, decisions, Phase 0 tasks |
| [13-auth-and-logging.md](13-auth-and-logging.md) | Login, JWT + refresh, policy-based authorization; middleware-only request/response logging |
| [14-up-and-coming.md](14-up-and-coming.md) | In-app "Up & Coming" tab + seed items (one-click launcher, LocalDB → full SQL migration) |

## Decisions so far (2026-07)

- **Frontend:** desktop-browser website only for v1; API built API-first (versioned REST + JWT) so a future mobile app or second site plugs in unchanged.
- **Base unit:** kg (South Africa); pack sizes convert to kg per product.
- **Payments:** card-only (plus EFT / on-account) — no cash handling; day close reconciles system total vs card machine batch.
- **In scope:** wastage tracking, own-use/gift/barter movements, chemical withholding-period enforcement.
- **Offline:** one app; ClientGuid idempotency built now, browser outbox later (doc 08).
- **Costing:** season estimate + season-end true-up (doc 09).
- **Backend:** .NET modular monolith, repository pattern + unit of work, EF Core code-first, SOLID (doc 11).
- **Database:** `(localdb)\MSSQLLocalDB` (SQL 2019 Express LocalDB, verified working) for development (doc 07).
- **Product range:** not only produce — prepared goods (coffee, cupcakes, sandwiches) with recipes/ingredient depletion, and plain resale merchandise (docs 01/02).
- **Auth:** login with JWT + rotating refresh tokens, deny-by-default policy-based authorization on every endpoint (doc 13).
- **Logging:** every API call + response logged via middleware/Serilog only — no logger calls in services or repositories (doc 13).
- **In-app "Up & Coming" tab** showing the live roadmap; first entries: one-click launcher, LocalDB → full SQL service migration (doc 14).

## The one-paragraph summary

The system tracks produce from **field to till**: what was planted and spent per block (farming costs), what was harvested or bought in (stock, in batches with grades and shelf life), what it cost (cost of sales — the hardest part for own-grown produce), what was sold and how (POS — must work offline), and what it all means at month end (reporting). The biggest design risks are unit-of-measure conversions, wastage/shrinkage tracking, costing own-grown produce, and making data capture easy enough that it actually happens in the field.
