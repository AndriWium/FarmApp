# 02 — Draft Data Model (MSSQL)

Entity-level draft, not final DDL. Names in PascalCase per .NET convention. All money columns `decimal(18,2)`, all quantities `decimal(18,3)` (you will sell 0.735 kg of tomatoes).

## Farming

```
Block            (BlockId, Name, AreaHectare, Notes, IsActive)
Crop             (CropId, Name)                          -- Peach, Tomato
Cultivar         (CultivarId, CropId, Name)              -- Transvaalia
Planting         (PlantingId, BlockId, CultivarId, StartDate, EndDate NULL,
                  Type: Annual|Perennial, PlantCount NULL, Notes)
Season           (SeasonId, PlantingId, Name, StartDate, EndDate)
                 -- for perennials: one Season per production year; for annuals
                 -- Planting ≈ Season, but keep the table for uniform costing

ActivityType     (ActivityTypeId, Name, Category)        -- Spray, Fertilise, Irrigate...
Activity         (ActivityId, SeasonId, ActivityTypeId, Date, LabourHours,
                  LabourCost, Notes)
ActivityInput    (ActivityInputId, ActivityId, InputItemId, Qty, UnitCost)
                 -- consumes input stock; snapshot the cost at time of use

RainfallLog      (Date, Mm, Notes)
```

## Stock — inputs

```
InputItem        (InputItemId, Name, Category: Seed|Fertiliser|Chemical|Packaging|Ingredient|Other,
                  Unit, ReorderLevel, WithholdingDays NULL,   -- chemicals only
                  ActiveIngredient NULL, IsActive)
InputPurchase    (InputPurchaseId, SupplierId, Date, InvoiceRef)
InputPurchaseLine(.., InputItemId, Qty, UnitCost)
-- on-hand = purchases − ActivityInput consumption ± adjustments (movement table
-- or computed; a movement table is more auditable)
```

## Stock — sellable products (produce, prepared goods, merchandise)

```
Product          (ProductId, Name, ProductType: Produce|Resale|Prepared,
                  CropId NULL,                            -- Produce only
                  MakeMode NULL: ToOrder|Batch,           -- Prepared only
                  BaseUnit: kg|each, IsActive)
Grade            (GradeId, Name)                          -- Class 1, Class 2, Juicing
                 -- GradeId is NULLABLE everywhere below: grades apply to produce,
                 -- not to cappuccinos
PackSize         (PackSizeId, ProductId, Name,            -- "1kg punnet", "10kg box"
                  QtyInBaseUnit)                          -- conversion factor. Critical.

RecipeLine       (RecipeLineId, ProductId,                -- the prepared product
                  InputItemId, Qty)                       -- ingredient + qty per 1 unit
                 -- recipe cost = Σ Qty × ingredient weighted-avg cost
ProductionBatch  (ProductionBatchId, ProductId, Date, QtyMade, MadeBy NULL,
                  IngredientCost)                         -- "baked 24 cupcakes":
                 -- consumes ingredients (input StockMovement per RecipeLine × QtyMade),
                 -- creates a finished-goods StockBatch at IngredientCost/QtyMade
                 -- Made-to-order products skip this: the SALE consumes ingredients
                 -- directly via the recipe (no finished-goods batch exists)

StockBatch       (StockBatchId, ProductId, GradeId NULL,
                  Source: Harvest|Purchase|Production,
                  HarvestId NULL, PurchaseLineId NULL, ProductionBatchId NULL,
                  Date, QtyIn, UnitCost,                  -- cost per base unit
                  ShelfLifeDays, BestBeforeDate computed)

Harvest          (HarvestId, SeasonId, Date, PickedBy NULL, Notes)
HarvestLine      (.., ProductId, GradeId, QtyKg)          -- creates StockBatch rows

ProducePurchase  (ProducePurchaseId, SupplierId, Date, InvoiceRef)
ProducePurchaseLine (.., ProductId, GradeId, Qty, UnitCost)  -- creates StockBatch

StockMovement    (StockMovementId, StockBatchId, Date, Type, Qty signed,
                  RefTable, RefId, Reason NULL, Location NULL)
  Type: HarvestIn, PurchaseIn, SaleOut, Wastage, OwnUse, Sample, Donation,
        Repack, Adjustment, TransferOut, TransferIn
Location         (LocationId, Name)                       -- Farm store, Market stall
StockTake        (StockTakeId, Date, LocationId, Notes)
StockTakeLine    (.., ProductId/BatchId, CountedQty, SystemQty, Variance)
```

**On-hand per batch = SUM(StockMovement.Qty).** Never store a mutable "quantity on hand" column as the source of truth; derive it (indexed view or maintained column updated only through the movement table).

## Costing

```
SeasonCostSummary (SeasonId, InputCost, LabourCost, OverheadAllocated,
                   TotalKgHarvested, CostPerKg)   -- recalculated job, not typed in
-- StockBatch.UnitCost for Harvest batches = SeasonCostSummary.CostPerKg at
-- harvest time; a season-end true-up adjustment revalues if materially different
```

## Sales / POS

```
Customer         (CustomerId, Name, Phone, Type: Retail|Wholesale|Account,
                  PriceListId, CreditLimit NULL)
PriceList        (PriceListId, Name)                      -- Retail, Wholesale
Price            (PriceId, PriceListId, ProductId, GradeId, PackSizeId,
                  UnitPrice, ValidFrom, ValidTo NULL)     -- keep history!

TillSession      (TillSessionId, LocationId, OpenedAt, OpenedBy, ClosedAt NULL,
                  SystemCardTotal NULL, CardMachineBatchTotal NULL,
                  Difference NULL, DifferenceNote NULL)   -- card-only day close
Sale             (SaleId, TillSessionId, CustomerId NULL, DateTime, Channel,
                  Status: Complete|Refunded, Notes,
                  ClientGuid UNIQUE)                      -- offline sync idempotency
SaleLine         (SaleLineId, SaleId, ProductId, GradeId, PackSizeId NULL,
                  Qty, UnitPrice, DiscountAmount, DiscountReason NULL,
                  CostAtSale)                             -- snapshot COS at sale time
SalePayment      (SalePaymentId, SaleId, Method: Card|EFT|Account, Amount)
                 -- card-only decision; enum extensible if cash ever returns
CustomerPayment  (CustomerPaymentId, CustomerId, Date, Amount, Method, Ref)
```

## Expenses & misc

```
ExpenseCategory  (ExpenseCategoryId, Name, IsFarmingDirect bit)
Expense          (ExpenseId, Date, ExpenseCategoryId, Amount, VatAmount NULL,
                  SupplierId NULL, SeasonId NULL,         -- optional block allocation
                  Notes, AttachmentPath NULL)             -- photo of the slip
Supplier         (SupplierId, Name, Phone, Notes)
AppUser          (UserId, Name, Pin/Hash, Role: Owner|Worker|Cashier)
AuditLog         (who, when, what, before/after)          -- at least for deletes/edits
```

## Design rules

1. **Movements, not mutations** — stock and money changes are append-only rows; balances are derived. Makes month-end reports and shrinkage detection trivial and honest.
2. **Snapshot costs and prices** onto transaction lines (`UnitCost`, `CostAtSale`, `UnitPrice`) — historical reports must not change when today's price changes.
3. **Everything sellable has a base unit (kg or each)** and all pack sizes convert to it. Reports aggregate in base units; the UI displays pack sizes.
4. **Soft-delete + audit** on master data; hard deletes only where nothing references the row.
5. **Dates are dates, money is decimal** — no floats, no datetimes where a date will do (harvests belong to a day, not 14:32:07).
