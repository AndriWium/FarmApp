-- Reporting.SalesAnalysis
--
-- Detail grain: one row per SaleLine, joined to Sale for Channel/DateTime/CustomerId and to
-- Product/Grade/PackSize for display names. Excludes Refunded sales entirely (Phase 2b's
-- balance/day-close precedent - a refunded sale must not appear in any report). Deliberately
-- left at line grain rather than pre-aggregated by product/grade/packsize/channel/customer/
-- day-of-week (doc 04 §2's whole list) - the API aggregates further as needed, which is more
-- flexible than baking one specific rollup into the view (doc 11: reports bypass repositories,
-- but a view baked to one aggregation would just move the same rigidity into SQL).
--
-- LineTotal/GrossMargin are computed entirely from the columns the line was already priced/
-- costed in (Qty/UnitPrice/DiscountAmount/CostAtSale) - never re-deriving base-unit quantities
-- via PackSize.QtyInBaseUnit, which would double-apply the pack-size conversion SaleLine already
-- baked into UnitPrice/CostAtSale at sale time (Phase 2a's own documented gotcha).
CREATE VIEW Reporting.SalesAnalysis AS
SELECT
    sl.SaleLineId,
    sl.SaleId,
    s.[DateTime]                                                   AS SaleDateTime,
    s.Channel,
    s.CustomerId,
    sl.ProductId,
    p.Name                                                         AS ProductName,
    sl.GradeId,
    g.Name                                                         AS GradeName,
    sl.PackSizeId,
    ps.Name                                                        AS PackSizeName,
    sl.Qty,
    sl.UnitPrice,
    sl.DiscountAmount,
    sl.DiscountReason,
    sl.CostAtSale,
    (sl.Qty * sl.UnitPrice - sl.DiscountAmount)                    AS LineTotal,
    (sl.Qty * sl.UnitPrice - sl.DiscountAmount) - (sl.CostAtSale * sl.Qty) AS GrossMargin
FROM dbo.SaleLines sl
JOIN dbo.Sales s ON s.SaleId = sl.SaleId
JOIN dbo.Products p ON p.ProductId = sl.ProductId
LEFT JOIN dbo.Grades g ON g.GradeId = sl.GradeId
LEFT JOIN dbo.PackSizes ps ON ps.PackSizeId = sl.PackSizeId
WHERE s.Status <> 'Refunded';
