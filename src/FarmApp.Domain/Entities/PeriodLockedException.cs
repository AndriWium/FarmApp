namespace FarmApp.Domain.Entities;

/// <summary>Thrown by PeriodLockInterceptor when a save touches an IPeriodLocked entity whose
/// BusinessDate falls in a Closed AccountingPeriod. A distinct type (rather than a generic
/// InvalidOperationException) so ExceptionHandlingMiddleware can map it to a clean 409 instead
/// of the catch-all 500 (doc 10 §1; gap identified and documented in Phase 4c's DECISIONS.md).</summary>
public class PeriodLockedException(int year, int month, string message) : Exception(message)
{
    public int Year { get; } = year;
    public int Month { get; } = month;
}
