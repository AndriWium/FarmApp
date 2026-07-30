# 09 — Costing Own-Grown Produce, Explained

The question this answers: **"what did one kg of my own tomatoes actually cost me to produce?"** — and why that's trickier than it sounds.

## Why bought-in produce is easy

You buy 100 kg of avos for R2,000 → cost is R20/kg. When you sell 30 kg, cost of sales is 30 × R20 = R600. If a later batch costs R22/kg, a weighted average blends them. Done. Nothing to discuss.

## Why own-grown is hard: the timing problem

You spend money for **months before** the first rand comes back, and harvest arrives in **dribs and drabs**, not all at once:

```
Aug   Sep   Oct   Nov   Dec   Jan   Feb
prep  plant spray spray pick  pick  pick
R800  R1500 R400  R400  ↓     ↓     ↓
                        120kg 450kg 230kg   (800 kg total season)
```

Total season cost: R3,100. Total harvest: 800 kg → **true cost R3.88/kg**.
But you only know that in February! Look what naive month-by-month maths does:

| Month | Cost spent so far | Kg picked so far | "Cost per kg" if computed naively |
|-------|------------------|------------------|------------------------------------|
| Nov (before harvest) | R3,100 | 0 | ÷0 — undefined |
| Dec | R3,100 | 120 | **R25.83/kg** — looks catastrophic |
| Jan | R3,100 | 570 | R5.44/kg — still overstated |
| Feb | R3,100 | 800 | R3.88/kg — the real number, at last |

If December's sales report used R25.83/kg, it would "prove" you sold every kg at a loss — nonsense that would push you into bad decisions (raising prices mid-glut, killing a profitable crop). The naive figure isn't wrong maths, it's the **wrong question**: profitability of a crop only truly exists at *season* level.

## The rule we'll use (decided in doc 01, restated here)

1. **During the season — use an estimate.** At each harvest, cost the batch at an *expected* cost per kg: `expected total season cost ÷ expected total season yield`. The expectation can start crude (last season's number, or your gut feel typed into the season record) and improve as the season progresses.
2. **Season end — true-up.** When the season closes, compute the real figure: `actual accumulated costs ÷ actual total kg`. The difference between "estimated cost charged to sales during the season" and "actual" is posted as one adjustment line in the closing month (shown separately on the report as "costing true-up", so it doesn't silently distort that month's margin).
3. **Two honest numbers, clearly labelled, on reports:**
   - *Monthly gross margin* on own-grown produce = sales − **estimated** COS → marked as provisional while the season is open.
   - *Season margin* per planting = all season revenue − all season cost → the real "should I plant this again?" number, final only after true-up.

## What goes into "season cost"

- **Inputs consumed** on that planting (seed, fertiliser, chemicals — captured via the activity log, at the price paid).
- **Direct labour** (activity hours × rate; your own hours at a notional rate so the crop isn't fake-profitable).
- **Direct packaging & transport** for that crop's produce.
- **Not** (initially): general overheads — electricity, insurance, bakkie repairs. These show *below* gross profit as expenses. Allocating them per crop is a later refinement; doing it too early adds arguing, not insight.
- **Not ever** (per doc 05 §18): orchard establishment costs (trees, trellising) in a single season's cost/kg — those are reported separately per block.

## Worked example, end to end

Season "Tomatoes 2026" on Block A. Expected: R3,000 cost, 750 kg → estimate **R4.00/kg**.

- Dec: pick 120 kg → batch costed at R4.00/kg. Sell 100 kg @ R15 → sales R1,500, COS R400, provisional margin R1,100.
- Jan: pick 450 kg → same R4.00/kg. (If mid-season it's clear yield will be poor, update the estimate; new batches use the new rate.)
- Feb: last pick 230 kg; season closes. Actual: R3,100 cost, 800 kg → **R3.88/kg** actual.
- True-up: sold kg were charged at R4.00 but actually cost R3.88 → COS was overstated by R0.12 × kg-sold; one credit adjustment in Feb, labelled "Tomatoes 2026 true-up".
- Season report: total revenue vs R3,100 → the real profitability of that planting, comparable against next season and against other crops.

## Implementation notes

- `Season` gets `ExpectedTotalCost`, `ExpectedYieldKg`, `EstimatedCostPerKg` (editable while open) and `Status: Open|Closed`.
- Harvest batches snapshot the estimate into `StockBatch.UnitCost`; sales snapshot into `SaleLine.CostAtSale` — historical rows never change (doc 02 rule 2).
- The true-up is a background-job calculation but a **user-confirmed** posting (show the number, owner clicks approve) — silent revaluations destroy trust in reports.
- Season close is blocked until all its harvests/wastage are captured, and is itself lockable/reopenable like months (doc 10).
