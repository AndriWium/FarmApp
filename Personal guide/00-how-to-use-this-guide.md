click "Ctrl + Shift + V" to view this in a better format

# Build-It-Yourself Guide — How To Use It

This folder is your **do-it-without-AI curriculum** for building the Farmers Application. The planning docs ([doc 01](<../AI Guide/01-scope-and-modules.md>) to [doc 14](<../AI Guide/14-up-and-coming.md>)) say *what* to build; these guides say *how, step by step, in what order*, assuming you want to learn as you go.

A note before anything else: you called yourself "not the smartest" — forget that. 
You're a working .NET/Angular/MSSQL developer planning a real system for a real business. 
What actually makes projects like this fail isn't brains, it's **skipped steps and vague goals**. 
So this guide fights back with structure: tiny steps, a checkpoint after every one, and never more than one new concept at a time. 
If you follow the checkpoints honestly, you cannot get badly lost.

## The guides

| File | Covers |
|------|--------|
| [01-sql-guide.md](01-sql-guide.md) | LocalDB, SSMS, reading the schema EF creates, writing report views, backups |
| [02-backend-guide.md](02-backend-guide.md) | Solution setup → EF code-first → repositories → API → auth → logging middleware |
| [03-frontend-guide.md](03-frontend-guide.md) | Angular workspace → layout → login → first CRUD screen → the patterns every later screen reuses |

## The build order (vertical slices, not layer-by-layer)

Do **not** build the whole database, then the whole backend, then the whole frontend. You'd spend weeks with nothing working, and motivation dies in the gap. Instead build in **slices** — each milestone produces something you can click:

| Milestone | What works at the end | Guide sections |
|-----------|----------------------|----------------|
| M1 | You can see your database in SSMS | SQL steps 1–4 |
| M2 | Solution builds; `FarmApp` DB exists with one table, created by a migration | BE steps 1–3 |
| M3 | Swagger shows a working `/api/v1/grades` CRUD | BE steps 4–6 |
| M4 | Angular app runs and lists grades from your API | FE steps 1–5 |
| M5 | Login works end to end; API rejects calls without a token | BE step 7, FE step 6 |
| M6 | Every API call appears in a structured log file | BE step 8 |
| M7 | All master data screens (products, blocks, customers…) | repeat M3+M4 patterns |
| M8+ | Stock → POS → farming → costing → reports | roadmap [doc 06](<../AI Guide/06-roadmap.md>), one phase at a time |

Milestones M1–M6 are the **teaching phase** — they introduce every pattern once, slowly. From M7 on, you're repeating known patterns with new tables, and speed comes naturally.

## Rules (these do the heavy lifting)

1. **Type, don't paste.** The snippets in these guides are for *reading and retyping*. Typing is where learning happens; pasting is where confusion hides.
2. **One step at a time, checkpoint every step.** Every step ends with "✅ Checkpoint". Don't move on until it passes. If a checkpoint fails, the problem is in the step you just did — a tiny search area. That's the whole trick.
3. **Commit at every checkpoint.** `git add -A` then `git commit -m "M2.3: first migration applied"`. Cheap time machine: when something breaks, `git diff` shows exactly what changed since it last worked.
4. **The 45-minute rule.** Stuck? Struggle for up to 45 minutes (struggling is learning). After that: read the error message *slowly, out loud* — the answer is in it more than half the time; then search the exact error text; then take a walk. Do not thrash for 3 hours — that teaches nothing but frustration.
5. **Keep a `LEARNING.md`** in the repo. One line whenever something clicks or bites you ("migrations are just C# files that write SQL", "CORS error = backend fine, browser blocked it"). You in 6 months will treasure this file.
6. **When guides and planning docs disagree, planning docs win** — then note it here. Doc [doc 12](<../AI Guide/12-implementation-handoff.md>)'s decisions table is law: card-only, kg, ClientGuid idempotency, deny-by-default auth, middleware-only logging.
7. **It's fine to build ugly first.** Working-and-ugly beats elegant-and-unfinished. Every pattern here gets refactored naturally as you repeat it.

## Your machine (verified 2026-07-26)

| Tool | Version | Status |
|------|---------|--------|
| .NET SDK | 10.x (9.0.201 also present) | ✅ ready — projects target net10.0 |
| Visual Studio | Community 2022 (17.13) | ⚠️ too old for .NET 10 — install latest VS Community (2026); CLI builds work meanwhile |
| Node.js / npm | 22.14.0 / 10.9.2 | ✅ ready |
| Angular CLI | 19.0.2 | ✅ ready |
| git | 2.45.1 | ✅ ready |
| SQL LocalDB | 2019 (`(localdb)\MSSQLLocalDB`) | ✅ ready ([doc 07](<../AI Guide/07-database-setup.md>)) |
| SSMS | 22 (installed via VS installer) | ✅ ready — SQL guide step 2 already done; just connect to `(localdb)\MSSQLLocalDB` |

## Where the code lives

Create the repo at **`H:\FarmApp\`** (short path, no spaces — saves you from a dozen weird tooling issues). First actions ever:

```powershell
mkdir H:\FarmApp
cd H:\FarmApp
git init
```

Then copy the planning docs in as `docs\` (so the spec travels with the code) and start the SQL guide.
