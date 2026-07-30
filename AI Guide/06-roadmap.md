# 06 — Roadmap

Phased so that **each phase is usable on its own** and produces real data from day one. Resist starting phase N+1 before phase N is in daily use — real usage will change the design more than any planning document.

## Phase 0 — Foundations (1–2 weekends)
- Solution + Angular workspace skeleton (doc 03 layout), EF Core migrations, auth (JWT + PIN), audit interceptor, automated backup job.
- Master data screens: products, grades, pack sizes, blocks, crops/cultivars, input items, suppliers, customers, price lists.
- Opening balance capture (stock take entry, debtor balances).
- **Exit test:** master data captured for the real farm; backup restore tested once.

## Phase 1 — Stock + wastage (the spine)
- Produce batches: harvest-in (simple form, no farming module yet), purchase-in.
- Stock movements: sale-out placeholder, wastage, own-use, repack, adjustment, transfer between two locations.
- Stock take with variance.
- Reports: on hand, movement balance, wastage.
- **Exit test:** a full week of real stock reality captured in under 10 min/day.

## Phase 2 — POS + cash-up
- Till sessions (float, cash-up, over/short), the touch sale screen, price lists, discounts with reasons, payments (cash/card/EFT/account), receipts, refunds.
- FIFO batch depletion; debtors (account sales + customer payments + statements).
- **Offline PWA + sync queue built in this phase, not after.**
- Reports: daily sales, till over/short, debtors aging.
- **Exit test:** one real market day run entirely on the system, offline, cash-up balances.

## Phase 3 — Farming capture
- Plantings/seasons, activity log with input consumption (depletes input stock), labour hours, rainfall log.
- Chemical withholding: lock/warn on harvest within window.
- Phone-first quick-capture screens (30-second rule, doc 05 §1).
- Reports: input usage, spray log/safety board, harvest summary per block.
- **Exit test:** two weeks of field activity captured from the phone, in the field.

## Phase 4 — Costing + month-end pack
- Season cost accumulation, cost per kg, harvest batch costing with season-end true-up.
- SaleLine.CostAtSale wiring → gross margin.
- Month-end close/lock; full report pack (doc 04): income statement, sales analysis, stock, farming, cash/debtors; CSV/Excel export.
- **Exit test:** one real month closed; every number on the pack drillable and explainable.

## Phase 5 — Polish and the parking lot
Only now: VAT reporting (if registering), scale/printer hardware integration, expense slip photos, accounting export, and doc 05 §20 items — each pulled in by an actual recurring pain, not by enthusiasm.

## Standing rules while building
1. You are the user — deploy early, use it for the real business from Phase 1, and let usage reorder this roadmap.
2. Every phase ships with its capture screens *and* its reports — capture without visible payoff doesn't survive.
3. Keep a `DECISIONS.md` — one line per design decision (costing rule, rounding rule, FIFO, month-lock policy). Future-you will need the "why".
