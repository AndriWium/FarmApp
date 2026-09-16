-- Reporting.HarvestSummaryBySeason
--
-- Detail grain: kg per crop/product/grade for one Season (doc 04 §4's "harvest summary: kg per
-- crop per grade vs last season"). Season -> Planting -> Cultivar -> Crop is a chain of plain FK
-- columns (no navigation properties anywhere in this codebase), so the crop each season belongs
-- to is resolved via three joins here rather than being denormalized onto Season itself.
-- "vs last season" (the same crop's most recent earlier season) is deliberately NOT computed in
-- this view - a SQL view can't easily self-reference "the previous row for this crop" without a
-- window function, and ReportQueries.GetHarvestSummaryAsync instead runs a second, tiny query to
-- find that prior SeasonId and re-queries this same view with it - two simple queries beats one
-- LAG()-window-function query, for a report this infrequently run.
CREATE VIEW Reporting.HarvestSummaryBySeason AS
SELECT
    s.SeasonId,
    s.Name          AS SeasonName,
    s.StartDate,
    s.EndDate,
    cr.CropId,
    cr.Name         AS CropName,
    hl.ProductId,
    p.Name          AS ProductName,
    hl.GradeId,
    g.Name          AS GradeName,
    SUM(hl.QtyKg)   AS QtyKg
FROM dbo.Seasons s
JOIN dbo.Plantings pl ON pl.PlantingId = s.PlantingId
JOIN dbo.Cultivars cv ON cv.CultivarId = pl.CultivarId
JOIN dbo.Crops cr ON cr.CropId = cv.CropId
JOIN dbo.Harvests h ON h.SeasonId = s.SeasonId
JOIN dbo.HarvestLines hl ON hl.HarvestId = h.HarvestId
JOIN dbo.Products p ON p.ProductId = hl.ProductId
LEFT JOIN dbo.Grades g ON g.GradeId = hl.GradeId
GROUP BY s.SeasonId, s.Name, s.StartDate, s.EndDate, cr.CropId, cr.Name, hl.ProductId, p.Name, hl.GradeId, g.Name;
