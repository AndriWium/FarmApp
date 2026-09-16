using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.AccountingPeriods;

/// <summary>Month-end close/reopen (doc 10 §1) - the first real use of the AccountingPeriod table
/// and PeriodLockInterceptor that have existed, unexercised, since Phase 0a. Standard layering
/// (interface + implementation, doc 11) - unlike Reports, this is real business logic/state
/// mutation with a clear single implementation, not a read-only aggregation query.</summary>
public interface IAccountingPeriodCloseService
{
    /// <summary>Read-only - safe to call any number of times while deciding whether to close
    /// (matches ISeasonCostingService.PreviewCloseAsync's precedent). Never fails: an
    /// out-of-range year/month simply returns a checklist with nothing to show.</summary>
    Task<CloseChecklistDto> GetCloseChecklistAsync(int year, int month, CancellationToken ct);

    /// <summary>Rejects with AccountingPeriodTillSessionsOpen if the checklist's one hard-blocking
    /// item fails (doc 10 §1 item 1); rejects with AccountingPeriodAlreadyClosed if this period
    /// was already closed. closedBy is the caller's own username (JWT claim), never
    /// client-supplied - matches TillSession.OpenedBy's precedent.</summary>
    Task<ServiceResult<CloseMonthResultDto>> CloseMonthAsync(int year, int month, string closedBy, CancellationToken ct);

    /// <summary>Owner-only at the controller (CanManageMasterData - doc 10 §1: "Owner role
    /// only"). Rejects with AccountingPeriodNotClosed if there's no Closed period for this
    /// [year, month] to reopen. reason is required (validated non-empty by
    /// ReopenMonthRequestValidator before this is ever called).</summary>
    Task<ServiceResult<AccountingPeriodDto>> ReopenMonthAsync(int year, int month, string reason, CancellationToken ct);
}
