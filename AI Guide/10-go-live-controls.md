# 10 — Go-Live & Integrity Controls Plan

The plan for the four "unglamorous but trust-deciding" items: month-end locking, VAT capture, opening balances, backups.

## 1. Month-end close & locking

**Goal:** once a month's report is printed/acted on, its numbers can never silently change.

- New table: `AccountingPeriod (PeriodId, Year, Month, Status: Open|Closed, ClosedAt, ClosedBy, ReopenedAt NULL, ReopenReason NULL)`.
- Every transactional insert/update validates its **date against the period status**. Writes into a Closed period are rejected with a clear message.
- **Close checklist** (a screen, not a memo) — closing runs these checks and shows red/green:
  1. All till sessions closed (day closes done, card differences explained).
  2. No Pending outbox items from any client (once offline sync exists).
  3. Stock movement balance check passes (opening + in − out ± adj = closing).
  4. Open seasons: estimated costs reviewed; closed seasons: true-up posted (doc 09).
  5. Wastage entries reviewed (not necessarily zero — reviewed).
- **Late paperwork rule:** an invoice found after close is captured **into the current open month** with its real document date stored in a `DocumentDate` field (so VAT/audit still sees the truth) — the closed month stays closed.
- **Reopening** is allowed but loud: Owner role only, reason required, audit-logged, and the report pack for that month is marked "reissued".
- Build in **Phase 4** (with the report pack), but the `AccountingPeriod` table and date-validation hook go in from Phase 0 — retrofitting the check into every write path later is painful.

## 2. VAT readiness (capture now, report later)

**Status today: not VAT-registered** (SA: compulsory at R1m turnover/12mo, voluntary from R50k).

- From day one, `Expense`, `InputPurchaseLine`, and `ProducePurchaseLine` carry `VatAmount` (nullable) — when capturing a supplier slip, typing the VAT shown costs 5 seconds and makes history reclaimable/reportable if registration comes.
- `Supplier.VatNumber NULL` field — needed on tax invoices later.
- Sales side: prices are simply prices for now. On registration: add `VatRate` to config, treat card prices as **VAT-inclusive** and back-calculate (`vat = total × 15/115`), receipts become tax invoices (need farm VAT number, "Tax Invoice" wording), and the VAT201 summary report (doc 04 §6) gets built. None of this needs schema changes if `VatAmount` columns exist from the start.
- **Trigger to watch:** the reporting module should show rolling-12-month turnover on the month-end pack with a warning when approaching R1m (and quietly note the R50k voluntary threshold has been passed).

## 3. Opening balances & go-live

**Goal:** day one of live use starts from truth, not from zero.

Go-live sequence (build as a guided one-time wizard in Phase 0–1):
1. **Master data first**: products, grades, pack sizes, blocks, crops, suppliers, customers, price lists.
2. **Produce stock take**: physical count → creates opening `StockBatch` rows (source: `Adjustment/Opening`) with best-guess cost and realistic remaining shelf life.
3. **Input stock take**: same for fertiliser/chemicals/packaging on hand.
4. **Debtors**: per account customer, amount currently owed → opening debtor entries (one line per customer is enough; per-invoice detail optional).
5. **Open seasons**: for each crop currently in the ground: costs spent so far (one summary amount is fine — "Tomatoes 2026: ±R1,800 to date"), expected total cost & yield → gives the costing estimate (doc 09) something to work with.
6. **Cut-over rule:** pick a date; everything before it lives only in the opening balances, everything after is captured transaction-by-transaction. No parallel running of old notebooks "just in case" beyond one month — dual systems mean both are wrong.

All opening entries are flagged `IsOpeningBalance` so reports can distinguish "brought forward" from "captured live".

## 4. Backups (and restore)

**Goal:** losing the PC or the server costs at most one day of data, and we *know* the restore works because it's been done.

Development (LocalDB):
- The DB is disposable-ish (migrations + seed rebuild it), but still: weekly copy of the `.mdf`/`.bak` to a second drive once real planning data goes in.

Production (whichever option from doc 07):
- **Nightly full backup** + hourly transaction log backups if using full recovery model (start simple: nightly full, simple recovery — a farm loses at most one day).
- **3-2-1 rule:** 3 copies, 2 media, 1 off-site. Practically: nightly `.bak` on the server + copy to cloud storage (Azure Blob / Backblaze / even OneDrive) + periodic copy on a USB drive kept off-premises.
- Cloud-hosted SQL (option 1 in doc 07) gives point-in-time restore out of the box — strongest argument for that option.
- **Automated restore test**: monthly job (or checklist item on the month-end close screen) — restore the latest backup to a scratch database, run a sanity query (row counts, latest sale date), record the result. An untested backup is a hope, not a backup.
- **What else to back up:** attachment files (receipt/slip photos), `appsettings` secrets (in a password manager, not in the repo), and the deployment notes.
- Non-DB safety net: the **audit log + append-only movements** design means partial recovery/reconstruction is possible even from an older backup.

## Build order summary

| Item | Skeleton (Phase 0) | Full feature |
|------|--------------------|--------------|
| Period locking | `AccountingPeriod` table + write-path date check | Close checklist screen — Phase 4 |
| VAT | `VatAmount`/`VatNumber` columns, captured on slips | VAT201 report — only on registration |
| Opening balances | Wizard | Used once at go-live (end Phase 1) |
| Backups | Scripted backup from first real data | Off-site + restore test — before go-live |
