# 05 — Things You've Likely Not Thought About

Ordered roughly by how much pain each causes if ignored.

## 1. Data capture friction will kill the system
The #1 reason farm software fails: recording an activity takes 3 minutes on a desktop, so it doesn't happen, so the data is incomplete, so the reports lie, so the system gets abandoned.
- Every frequent capture (harvest, spray, sale, wastage) must be doable on a **phone in under 30 seconds**, offline, with big buttons and smart defaults (last block used, today's date, usual products).
- Accept approximate data over no data — "±5 crates" beats an empty log.
- Design the *capture* screens before the *report* screens.

## 2. Units of measure and pack sizes
You harvest in crates, weigh in kg, sell in punnets, bags, boxes, and loose kg. Without a single base unit per product and explicit conversion factors, stock counts and margins become fiction. Also: a "crate" of peaches ≠ a "crate" of cabbage — conversions are per product. Repacking (10kg box → 10 × 1kg punnets) is a stock movement, not a new product magically appearing.

## 3. Wastage is a first-class citizen, not an afterthought
For fresh produce, 10–30% shrinkage is normal. If the system only tracks "in" and "sold", the difference silently disappears and gross margin is overstated. Make throwing stock away a **2-tap action with a reason** (rotten, damaged, unsold, eaten by birds). The wastage report is where a fruit seller finds real money.

## 4. Own use, gifts, staff, barter
Farm reality: the household eats stock, workers get some, neighbours trade a box of peaches for eggs. If these aren't recordable movements, they show up as unexplained shrinkage (or worse, suspected theft). Add movement types: OwnUse, StaffRation, Gift/Sample, Barter (with estimated value), Donation.

## 5. Chemical withholding periods (food safety + legal)
After spraying, produce may not be harvested/sold for N days (the withholding period / PHI on the label). This is a legal food-safety requirement, not admin. The system should:
- store withholding days per chemical,
- compute "block locked until DATE" from each spray record,
- warn (and log overrides) when a harvest is entered inside the window.
This one feature can prevent a serious incident and is a genuine differentiator.

## 6. Batch traceability
If a shop or customer reports bad produce, can you answer "which block, harvested when, sprayed with what"? The batch model (harvest → batch → sale) gives you this for free — but only if the POS depletes *specific batches* (FIFO), not just product totals. Also matters for any future GlobalGAP/retailer certification.

## 7. Offline is not optional
Market stalls, fields, and load shedding mean the POS and field capture must work with zero connectivity and sync later. Retrofitting offline onto an online-only app is close to a rewrite — commit to the PWA/queue architecture (doc 03) from day one.

## 8. Cash controls
**DECISION (2026-07): card-only transactions — no cash handling.** This eliminates the float/cash-up/payout problem entirely. The remaining control is the **day close**: system card total vs card machine settlement batch, difference recorded with a note. If cash is ever accepted later, the classic controls (float, cash-up, over/short per cashier, recorded payouts) must come back with it.

## 9. Costing timing problem for own-grown produce
You spend money on a crop for months *before* the first harvest, and harvests trickle in over weeks. Cost per kg at first harvest is mathematically huge and meaningless. Solutions: use a running season estimate during the season and do a **season-end true-up**; report "season margin" as the honest number and treat monthly gross margin on own-grown produce as an estimate. Decide and document this rule before writing costing code.

## 10. Prices change constantly
Fruit prices move weekly (glut vs scarcity). Price history must be kept (Price.ValidFrom/To) so old sales report against the price that was actually charged, and so you can see price vs volume over a season. Quick "change today's price" must be a 10-second job or people will bypass it with manual discounts.

## 11. Month-end lock and back-dating
Someone will find a forgotten invoice on the 5th for last month. Rule: months get closed; late entries either go into the current month or the closed month is explicitly reopened (logged). Without this, printed reports and the system disagree forever.

## 12. VAT decision up front
In South Africa, VAT registration is compulsory above R1m turnover/12 months and optional above R50k. Even if not registered now: capture VAT on expense/purchase slips from day one (it's cheap) so a later registration doesn't require re-capturing history. If registered: prices at the stall are VAT-inclusive — the system must back-calculate.

## 13. Opening balances and go-live
The system starts mid-life: stock already on hand, customers already owing, a season already in progress with money already spent. Build an explicit opening-balance capture (stock take + debtor balances + season costs to date) or the first months of reports will be garbage and trust dies early.

## 14. Backups and the bus factor
This becomes the business's memory. Automated off-site backup with a **tested restore**, before go-live, non-negotiable. Also: at least one other person must know how to log in and run the basics.

## 15. Multi-location stock
Stock at the farm ≠ stock on the bakkie ≠ stock at the market stall. Even two locations ("farm" and "market") with transfer movements will explain many "missing" items. Don't build warehouse management; do build locations + transfers.

## 16. Weather/rainfall as context
Cheap to capture, and the only way to answer "why was March terrible?" two years later. One number per day.

## 17. Scale/hardware integrations — defer, but leave the door open
Weighing scale integration, barcode/PLU labels, receipt printers: all real, none needed for v1. Manual weight entry + a cheap Bluetooth/USB thermal printer covers 90%. Keep the POS payment/receipt code behind interfaces so hardware can plug in later.

## 18. Perennials vs annuals cost differently
An orchard's establishment costs (trees, trellising) span decades — don't dump them into one season's cost per kg. Simplest honest treatment: keep establishment costs as a separate category reported per block, and only put *annual running costs* into cost per kg. (Full depreciation/amortisation is accountant territory — provide the data, don't build the accounting.)

## 19. What this system is NOT (scope guardrails)
- Not a general ledger / accounting package — it produces management figures and exports for the accountant (or later, a Xero/Sage export). Don't rebuild double-entry bookkeeping.
- Not payroll — labour hours × rate for costing only; actual payroll stays outside.
- Not a webshop — maybe a later phase; don't design v1 around it.
- Not multi-farm SaaS — single tenant (doc 03).

## 20. Nice-to-haves parking lot (resist until the core works)
Photos per block over time · pest/disease scouting log · irrigation scheduling · customer WhatsApp order capture · loyalty for regulars · yield forecasting · packhouse labels · accounting export · GlobalGAP document pack.
