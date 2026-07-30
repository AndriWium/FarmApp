# 04 — Reporting

Two kinds of reports: **month-end** (the formal pack) and **operational** (daily/weekly screens that prevent losses). Every report below lists the data it depends on — this is the checklist that proves the data model captures enough.

## Month-end pack

### 1. Income statement (management view)
```
Sales (by channel)                      ← Sale/SaleLine
− Cost of sales                         ← SaleLine.CostAtSale
− Wastage                               ← StockMovement type Wastage × batch cost
= Gross profit (and GP%)
− Expenses (by category)                ← Expense
= Net profit
```
Shown per month with year-to-date and same-month-last-year columns.

### 2. Sales analysis
- By product / grade / pack size / channel / customer / day-of-week.
- Average selling price per kg per product vs last month (spot price drift).
- Discount & markdown totals with reasons (is "old stock" markdown growing? → stock buying/harvest timing problem).

### 3. Stock report
- **On hand, valued** (qty × batch cost) per product/grade/location.
- **Movement summary**: opening + harvest-in + purchases − sales − wastage − own use ± adjustments = closing. If this doesn't balance, the report must say so loudly.
- **Wastage/shrinkage report**: wastage by product with % of intake; stock-take variances. This is the profit-leak detector — arguably the most valuable report in the system.
- **Aging**: batches past or near best-before still on hand.

### 4. Farming report
- Per block/season: costs to date (inputs, labour, overhead), kg harvested, cost per kg, revenue attributable, margin. The "should I plant this again?" table.
- Harvest summary: kg per crop per grade vs last season.
- Input usage summary (what was consumed, what it cost).
- Rainfall for the month vs historical.

### 5. Cash & debtors
- Till sessions: over/short per session, per cashier, trend.
- Cash flow summary: cash in (sales, debtor payments) vs cash out (expenses, purchases).
- Debtors aging (current / 30 / 60 / 90+) with per-customer statements printable/sendable.

### 6. VAT summary (only when VAT-registered)
- Output VAT on sales, input VAT on purchases/expenses, net payable. Requires VAT fields captured on expenses/purchases from day one *if* registration is planned — decide early (see doc 05).

## Operational screens (not month-end, but daily value)

- **Today at the till**: live sales, payment split, items running low at the stall.
- **Sell-first list**: batches nearest best-before, suggested markdowns.
- **Spray safety board**: blocks currently inside a withholding period — with the date each becomes harvestable. Blocks harvest entry on locked blocks (warn + override with reason, and log it).
- **Stock to load** for tomorrow's market (pick list from on-hand).
- **Low input stock** (below reorder level).

## Design notes

- Build reports on **SQL views** (`Reporting` schema), one view per report section; Angular renders tables/charts from clean API DTOs.
- Every month-end number must be **drillable**: click a total → see the transactions. Trust in the system dies the first time a number can't be explained.
- Reports are **period-locked**: closing a month freezes it (no back-dated edits without an explicit, logged reopening). Otherwise last month's report changes after you've acted on it.
- Export: CSV/Excel for everything (accountant will ask), PDF for statements/invoices.
- Seasonality: month-vs-same-month-last-year comparisons matter more than month-vs-last-month for farming — build the comparison into the queries from the start.
