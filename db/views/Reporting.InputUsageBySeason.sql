-- Reporting.InputUsageBySeason
--
-- Detail grain: one row per ActivityInput, joined up to its Activity (for SeasonId/Date) and
-- InputItem (for display name) - doc 04 §4's "input usage summary: what was consumed, what it
-- cost". ActivityInput.UnitCost is already the snapshotted cost at time of use (doc 02), never
-- looked up again, so Qty * UnitCost here is exactly what that consumption actually cost -
-- matches Reporting.WastageValue/ExpensesByCategory's precedent of exposing detail-grain rows and
-- letting the caller GROUP BY whatever the report screen needs (per season, per input item, per
-- date range).
CREATE VIEW Reporting.InputUsageBySeason AS
SELECT
    ai.ActivityInputId,
    a.SeasonId,
    a.[Date],
    ai.InputItemId,
    ii.Name             AS InputItemName,
    ai.Qty,
    ai.UnitCost,
    ai.Qty * ai.UnitCost AS Cost
FROM dbo.ActivityInputs ai
JOIN dbo.Activities a ON a.ActivityId = ai.ActivityId
JOIN dbo.InputItems ii ON ii.InputItemId = ai.InputItemId;
