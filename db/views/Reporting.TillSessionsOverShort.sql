-- Reporting.TillSessionsOverShort
--
-- Detail grain: one row per CLOSED TillSession, for doc 04 §5's "till sessions: over/short per
-- session, per cashier, trend". Only closed sessions are included (Difference is only ever
-- populated by TillSessionService.CloseAsync's day close) - an open session has nothing to report
-- yet. OpenedBy stays the raw AppUser id (no username join) - matches TillSessionDto's own
-- precedent (doc 11: no IAppUserRepository is exposed outside Auth/TokenService for such a
-- lookup, see DECISIONS.md Phase 3b).
CREATE VIEW Reporting.TillSessionsOverShort AS
SELECT
    t.TillSessionId,
    t.LocationId,
    t.OpenedAt,
    t.OpenedBy,
    t.ClosedAt,
    t.SystemCardTotal,
    t.CardMachineBatchTotal,
    t.Difference,
    t.DifferenceNote
FROM dbo.TillSessions t
WHERE t.ClosedAt IS NOT NULL;
