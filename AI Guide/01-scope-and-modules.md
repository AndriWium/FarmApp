# 01 — Scope and Modules

## Module 1: Farming (small scale)

The goal: know **what each block/crop actually costs and yields**, so cost of sales and profitability reports mean something.

### Land & crops
- **Blocks / fields / orchards** — named areas with size (ha or m²). Even a small farm should split land into blocks, because costs and yields are compared per block.
- **Crops and cultivars** — e.g. "Peaches → Transvaalia", "Tomatoes → Rodade". Cultivar matters because prices and yields differ.
- **Plantings / seasons** — a crop instance on a block with a start date (planting) and lifecycle. Annuals (vegetables) get a new planting per season; perennials (fruit trees) are long-lived plantings with yearly production cycles.

### Activity log (the diary)
Every activity recorded against a block/planting with date, labour hours, and inputs used:
- Soil prep, planting/sowing
- Irrigation (duration/volume if measurable)
- Fertilising (product, quantity)
- Spraying (product, quantity, target pest/disease) — **must record the withholding period** (see doc 05)
- Pruning, thinning, weeding, mulching
- Harvesting (links to stock intake)

### Inputs & resources
- Input purchases (seed, seedlings, fertiliser, chemicals, packaging) go into **input stock**; activities *consume* input stock — this is how costs flow to blocks.
- Labour: hours per activity × rate (own time can have a notional rate; workers a real rate).
- Equipment & fuel: at minimum a fuel/repairs expense category allocated to blocks; full asset management is a later phase.

### Environment
- Rainfall log (per day, mm) — cheap to capture, very valuable when explaining a bad season.
- Optional: min/max temperature, frost events, hail events.

### Harvest
- Harvest entry: date, block/planting, quantity picked, into which grades (Class 1 / Class 2 / juicing / waste), by whom.
- Harvest creates **produce stock batches** — this is the seam between the farming module and the stock module.

---

## Module 2: Stock

There are **two very different kinds of stock** — don't model them as one thing:

### A. Input stock (fertiliser, seed, chemicals, packaging)
- Behaves like normal inventory: purchased, consumed by activities, counted.
- Needs supplier, purchase price, quantity on hand, reorder level.
- Chemicals need extra fields: registration/active ingredient, withholding period (days), safety notes.

### B. Produce stock (what you sell)
- **Perishable.** Every batch has a harvest/receipt date and a realistic shelf life. Old stock must be visible so it gets marked down or written off *before* it rots.
- **Batch/lot based.** A batch = one harvest event (own-grown) or one purchase (bought-in). Batches carry: source (block or supplier), date, grade, cost, quantity.
- **Own-grown vs bought-in** are both sellable stock but cost differently (see Module 3).
- **Grades**: the same fruit at different grades is effectively different products with different prices (Class 1 vs Class 2 vs juicing).
- **Units of measure**: harvested in kg or crates, sold per kg / per punnet / per bag / per box. Every product needs defined pack sizes with conversion factors to a base unit (kg). This is a top-3 source of bugs and bad data — design it early.
- **Stock movements** — every change is a typed movement: harvest-in, purchase-in, sale-out, wastage/spoilage, own-use, gift/sample, donation, repack (crate → punnets), adjustment (stock take), transfer (farm → market stall).
- **Stock takes**: periodic counts with variance recorded as adjustments — the variance report is your shrinkage/theft detector.

### C. Prepared goods & other merchandise (coffee, cupcakes, sandwiches, …)

The stall sells more than produce. These are **products with a recipe instead of a harvest**:

- **Ingredients** live in input stock (new category `Ingredient`: coffee beans, milk, flour, bread, cups, serviettes) — purchased like fertiliser, consumed by making/selling the item.
- Each prepared product has an optional **recipe** (bill of materials): e.g. 1 cappuccino = 18 g beans + 150 ml milk + 1 cup. The recipe gives automatic ingredient depletion *and* an automatic cost per unit.
- Two making modes:
  - **Made-to-order** (coffee, sandwiches): no finished-goods stock exists; the *sale itself* consumes the recipe ingredients. Nothing to count at day end except ingredients.
  - **Made-in-batch** (cupcakes): a **production entry** ("baked 24 cupcakes") consumes ingredients and creates a finished-goods `StockBatch` (short shelf life, wastage rules apply exactly like produce).
- **Plain merchandise** (bought ready-to-sell: bottled water, honey from a neighbour) is just resale stock — the existing bought-in model covers it, no recipe.
- Grades don't apply to these — grade becomes optional per product.

---

## Module 3: Cost of sales

### Bought-in produce & merchandise (easy case)
- Weighted average cost per product per batch. COS = qty sold × average cost.

### Prepared goods (easy-ish case)
- Cost per unit = **recipe cost**: sum of ingredient quantities × current ingredient cost (weighted average of ingredient purchases).
- Made-to-order items snapshot recipe cost at sale time; batch-made items snapshot it into the production batch.
- Recipe quantities won't be exact in practice (a splash more milk) — periodic ingredient stock takes catch the drift as shrinkage, same mechanism as produce.

### Own-grown produce (hard case — decide the model early)
Options, in increasing accuracy and effort:
1. **Season-level allocation (recommended start):** accumulate all direct costs (inputs consumed + labour + allocated overheads) per planting/season. At each harvest, cost per kg = season cost to date ÷ total kg harvested to date (recalculated at season end for the true figure). Simple, defensible, good enough for management decisions.
2. Standard costing with variance analysis — later, if ever.
3. Full biological-asset accounting (IAS 41 style) — not worth it at this scale; note only.

### What counts as cost
- **Direct:** inputs consumed on the block, direct labour, packaging, transport to market.
- **Overheads to allocate (or at least report separately):** fuel, electricity/water, repairs, rent, insurance, license fees. Start by *not* allocating — show them as expenses below gross profit — and add allocation later if needed.
- **Wastage is a cost:** spoiled stock must hit COS (or a separate "wastage" line — separate line is better, it keeps gross margin honest and makes waste visible).

### Outputs this module must support
- Gross margin per product, per grade, per channel, per month.
- Cost per kg per block/planting per season (the "should I even grow this?" number).

---

## Module 4: Point of sale

### Sales channels (all four exist for a farmer/fruit seller)
1. **Farm stall / gate sales** — walk-in retail.
2. **Farmers market** — mobile, possibly no connectivity → POS must be offline-capable.
3. **Wholesale** — shops, restaurants, hawkers; different (lower) price list, often on account (invoice, pay later).
4. **Informal/bulk** — bakkie loads, negotiated prices.

### POS features
- Fast product grid (photos/big buttons — used with dirty hands in a hurry). Sells everything side by side: produce, prepared items (coffee, cupcakes, sandwiches), and merchandise — one basket, one payment.
- Price **per unit** and **per weight** (kg price × weighed amount; manual weight entry first, scale integration later).
- Multiple price lists (retail / wholesale) + per-line discount and markdown reasons ("old stock", "bulk deal", "regular customer").
- Payment types — **DECISION: card-only (plus EFT and on-account)**. No cash handling. This removes float, change calculation, and cash-security complexity entirely.
- Account customers: customer record, credit sales create debtor entries, record payments against them.
- Receipts: printed (58/80mm thermal) or shared digitally; invoices for wholesale.
- Returns/refunds (rare but must exist, with reason).
- **Day close**: at end of each trading day/session, compare the system's card-sales total against the card machine's settlement batch total; record any difference with a note. This replaces the classic cash-up and is the error/fraud control for a card-only operation.
- Each sale line depletes produce stock (oldest batch first — FIFO by default).

---

## Module 5: Reporting (month end)

See [04-reporting.md](04-reporting.md) for the full report list. Headlines:
- Income statement view: Sales − COS = Gross profit − Expenses = Net profit.
- Sales analysis by product / grade / channel / day.
- Stock: on hand (valued), movement summary, wastage report, aging.
- Farming: cost and yield per block, harvest summary, spray log (compliance).
- Cash: till over/short history, cash flow summary.
- Debtors: who owes what, aging.
- VAT summary (if/when VAT registered).
