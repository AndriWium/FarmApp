# 14 — "Up & Coming" (in-app tab + backlog)

## The feature itself

The finished application gets an **"Up & Coming" tab** — visible in the main navigation — showing what's planned/being built next. Purpose: the roadmap lives *in* the product, visible at the till and in the office, not buried in docs.

**Implementation (deliberately simple):**
- A `RoadmapItem` table (`RoadmapItemId, Title, Description, Status: Planned|InProgress|Done, SortOrder, TargetPhase NULL, CompletedOn NULL`) + Owner-only CRUD screen; the tab itself is read-only for everyone (policy `CanViewReports` not required — all roles see it).
- Done items stay visible (struck through / "recently shipped" section) for a month — seeing progress builds trust in the system.
- Seeded from the list below; from then on it's maintained in-app, and doc 06's parking lot graduates items into it.

## Seed entries

### 1. One-click launcher (start the app + SQL together)

**Goal:** starting the whole system must be one double-click — no terminal knowledge, usable by anyone in the family/on the farm.

Phased approach:
- **v1 — script + shortcut (cheap, do first):** a `Start-FarmApp.ps1` that:
  1. `sqllocaldb start MSSQLLocalDB` (idempotent — instant if already running);
  2. starts the published API (`FarmApp.Api.exe`) if not already running (the API serves the built Angular app as static files, so there is no separate FE server in production mode);
  3. waits for the health endpoint (`/health`) to answer;
  4. opens the default browser at `http://localhost:5000`.
  Wrapped in a Desktop/Start-menu shortcut with the farm logo as icon. A matching `Stop-FarmApp.ps1` for clean shutdown.
- **v2 — tray application (nicer):** a tiny .NET WinForms/WPF tray app: green/red status dot, Start/Stop/Open buttons, "view logs" link. Same logic as the script, friendlier face.
- **v3 — no launcher needed (best):** API installed as a **Windows Service** (`sc create` / `dotnet publish` + `UseWindowsService()`), auto-starting with the PC. Note: LocalDB *cannot* be started by a service reliably (it's a user-process instance) — v3 therefore depends on item 2 below (full SQL service), after which "starting the application" stops being a thing anyone does.

### 2. Investigate: moving from LocalDB to a full SQL Server service

**Why move (the triggers — revisit when any becomes true):**
- More than one PC/user needs the system at the same time (LocalDB is single-user, not network-accessible by default);
- The API should run as a Windows Service / auto-start (see item 1 v3);
- Real production use begins and scheduled backups must run unattended.

**Target:** SQL Server 2022 **Express** (free, same 10 GB limit as LocalDB, runs as a proper Windows service, network-capable). Install steps already documented in [07-database-setup.md](07-database-setup.md) production option 2.

**Migration plan (LocalDB → Express) — the actual investigation result:**
1. Install SQL Express (`.\SQLEXPRESS` instance), enable TCP/IP, fixed port, firewall rule.
2. Move the database — two equally valid options:
   - **Backup/restore:** `BACKUP DATABASE FarmApp TO DISK=...` on LocalDB → `RESTORE` on Express (works because both are SQL 2019+ era engines; restore *up* a version is fine, never down); or
   - **Recreate + reseed:** run EF migrations against the new server (`dotnet ef database update`) and bulk-copy data — cleaner if the move happens early with little data.
3. Create an app SQL login (least privilege, `db_owner` on FarmApp only, strong password in user-secrets/env — not in the repo).
4. Change **one line** — the connection string. Code-first EF means nothing else in the codebase changes at all.
5. Set up Task Scheduler backup job (Express has no SQL Agent) per doc 10 §4; verify a restore.
6. Decommission: `sqllocaldb stop MSSQLLocalDB` and keep the old `.mdf` for a month as a safety copy.

**Alternative also worth pricing at that point:** Azure SQL Basic (~a few hundred rand/month) — removes the farm-PC single point of failure and gives managed backups; pairs with cloud-hosting the API (doc 03).

### 3+ — graduated from the parking lot (doc 06 Phase 5 / doc 05 §20)
Seed the rest of the tab with, in no order: browser outbox for offline POS (doc 08) · phone-friendly capture screens · scale & receipt-printer integration · VAT201 report on registration · accounting export (Xero/Sage CSV) · photos per block · WhatsApp order capture · customer loyalty.
