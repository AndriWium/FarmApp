-- Reporting.SaleOutDepletion
--
-- Detail grain: one row per SaleOut StockMovement, with the SeasonId of the Harvest that sourced
-- its StockBatch when the batch came from a harvest (NULL when the batch was Purchase/Production-
-- sourced instead - StockBatch.HarvestId is only ever populated for Source = Harvest, per the
-- loose-nullable-pointer convention, so the LEFT JOIN naturally resolves to NULL for the other
-- two sources without needing an explicit Source = 'Harvest' guard).
--
-- This is the building block for doc 04 §4's revenue-attribution approximation (task brief):
-- SaleLine (has price) and StockMovement (has batch, hence season) aren't directly linked by a
-- shared key - a sale line never records which batch(es) FIFO depletion actually touched. The
-- approximation instead compares, per Product/Grade over a date window: how much quantity was
-- depleted from THIS season's harvest batches (WHERE SeasonId = @seasonId) against how much was
-- depleted in total (ignoring the SeasonId filter, i.e. summing this same view's rows regardless
-- of source) - see ReportQueries.GetSeasonRevenueAttributedAsync. QtyDepleted is positive (Qty on
-- a SaleOut movement is negative; this view negates it back to a plain depleted-quantity figure).
--
-- Joined to Sales and filtered to Status <> 'Refunded', matching Reporting.SalesAnalysis's own
-- filter exactly (StockMovementService always writes RefTable = 'Sale'/RefId = SaleId on every
-- SaleOut movement it creates). Without this filter a refunded sale's depletion would still count
-- toward the TotalQty denominator (a refund is a separate reversing entry, not a deletion of the
-- original SaleOut row) while its revenue is correctly excluded from SalesAnalysis - silently
-- understating the attributed proportion for any product a refund ever touched. Verified live
-- (Phase 4c): before this filter, a refunded 4kg sale of Product 1 counted in TotalQty with no
-- matching revenue; after it, the same product's revenue attribution excludes that sale entirely.
CREATE VIEW Reporting.SaleOutDepletion AS
SELECT
    m.StockMovementId,
    m.[Date],
    b.ProductId,
    b.GradeId,
    -m.Qty      AS QtyDepleted,
    h.SeasonId  AS SeasonId
FROM dbo.StockMovements m
JOIN dbo.StockBatches b ON b.StockBatchId = m.StockBatchId
JOIN dbo.Sales s ON s.SaleId = m.RefId AND m.RefTable = 'Sale'
LEFT JOIN dbo.Harvests h ON h.HarvestId = b.HarvestId
WHERE m.[Type] = 'SaleOut' AND s.Status <> 'Refunded';
