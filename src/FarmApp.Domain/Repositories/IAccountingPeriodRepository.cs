using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

/// <summary>Backs month-end close/reopen (doc 10 §1, Phase 4c). The AccountingPeriod table and
/// PeriodLockInterceptor have existed since Phase 0a; this is the first repository giving the
/// Application layer real read/write access to it, rather than only the interceptor's own
/// internal query.</summary>
public interface IAccountingPeriodRepository
{
    /// <summary>Tracked - CloseMonthAsync/ReopenMonthAsync mutate Status/ClosedAt/ClosedBy/
    /// ReopenedAt/ReopenReason on the result in place.</summary>
    Task<AccountingPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken ct);

    Task AddAsync(AccountingPeriod period, CancellationToken ct);
}
