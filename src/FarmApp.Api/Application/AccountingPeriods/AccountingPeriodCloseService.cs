using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.Reports;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.AccountingPeriods;

public class AccountingPeriodCloseService(
    IAccountingPeriodRepository periodRepo,
    ITillSessionRepository tillSessionRepo,
    IStockTakeLineRepository stockTakeLineRepo,
    ISeasonRepository seasonRepo,
    ISeasonCostSummaryRepository summaryRepo,
    ReportQueries reportQueries,
    IUnitOfWork uow) : IAccountingPeriodCloseService
{
    public async Task<CloseChecklistDto> GetCloseChecklistAsync(int year, int month, CancellationToken ct)
    {
        var periodStart = new DateTime(year, month, 1);
        var periodEndExclusive = periodStart.AddMonths(1);

        // Item 1 (doc 10 §1) - the one hard block (task brief).
        var openSessions = await tillSessionRepo.GetOpenSessionsOpenedInPeriodAsync(year, month, ct);
        var openSessionRows = openSessions
            .Select(s => new OpenTillSessionRowDto(s.TillSessionId, s.LocationId, s.OpenedAt, s.OpenedBy))
            .ToList();
        var tillSessionsAllClosed = openSessionRows.Count == 0;

        // Item 3 (doc 10 §1) - the "more meaningful" interpretation: material stock-take
        // variances, not the always-true balance identity (see DECISIONS.md). Advisory.
        var variances = await stockTakeLineRepo.GetNonZeroVariancesInPeriodAsync(year, month, ct);
        var varianceRows = variances
            .Select(v => new StockTakeVarianceRowDto(v.StockTakeId, v.Date, v.StockTakeLineId, v.StockBatchId, v.Variance))
            .ToList();

        // Item 4 (doc 10 §1), both halves. Advisory.
        var overlappingSeasons = await seasonRepo.GetSeasonsOverlappingPeriodAsync(year, month, ct);
        var openSeasonsNeedingEstimate = new List<SeasonReviewRowDto>();
        var closedSeasonsMissingTrueUp = new List<SeasonReviewRowDto>();

        foreach (var season in overlappingSeasons)
        {
            if (season.Status == SeasonStatus.Open && season.EstimatedCostPerKg is null)
            {
                openSeasonsNeedingEstimate.Add(new SeasonReviewRowDto(
                    season.SeasonId, season.Name, season.Status.ToString(), season.EstimatedCostPerKg, season.EndDate));
            }
            else if (season.Status == SeasonStatus.Closed && season.EndDate >= periodStart && season.EndDate < periodEndExclusive)
            {
                var summary = await summaryRepo.GetBySeasonIdAsync(season.SeasonId, ct);
                if (summary is null)
                {
                    closedSeasonsMissingTrueUp.Add(new SeasonReviewRowDto(
                        season.SeasonId, season.Name, season.Status.ToString(), season.EstimatedCostPerKg, season.EndDate));
                }
            }
        }

        // Item 5 (doc 10 §1) - purely informational, always "green" (task brief: "the checklist's
        // job here is surfacing the list, not gatekeeping it").
        var wastageEntries = await reportQueries.GetWastageInPeriodAsync(year, month, ct);

        return new CloseChecklistDto(
            year, month,
            tillSessionsAllClosed, openSessionRows,
            varianceRows,
            openSeasonsNeedingEstimate,
            closedSeasonsMissingTrueUp,
            wastageEntries.ToList(),
            CanClose: tillSessionsAllClosed);
    }

    public async Task<ServiceResult<CloseMonthResultDto>> CloseMonthAsync(int year, int month, string closedBy, CancellationToken ct)
    {
        var checklist = await GetCloseChecklistAsync(year, month, ct);
        if (!checklist.CanClose)
        {
            return ServiceResult<CloseMonthResultDto>.Fail(
                ServiceError.AccountingPeriodTillSessionsOpen,
                $"{checklist.OpenTillSessions.Count} till session(s) opened in {year:D4}-{month:D2} " +
                $"are still open: {string.Join(", ", checklist.OpenTillSessions.Select(s => $"#{s.TillSessionId}"))}.");
        }

        var period = await periodRepo.GetByYearMonthAsync(year, month, ct);
        if (period is null)
        {
            period = new AccountingPeriod { Year = year, Month = month };
            await periodRepo.AddAsync(period, ct);
        }
        else if (period.Status == AccountingPeriodStatus.Closed)
        {
            return ServiceResult<CloseMonthResultDto>.Fail(ServiceError.AccountingPeriodAlreadyClosed);
        }

        period.Status = AccountingPeriodStatus.Closed;
        period.ClosedAt = DateTime.UtcNow;
        period.ClosedBy = closedBy;

        await uow.SaveChangesAsync(ct);

        return ServiceResult<CloseMonthResultDto>.Ok(new CloseMonthResultDto(ToDto(period), checklist));
    }

    public async Task<ServiceResult<AccountingPeriodDto>> ReopenMonthAsync(int year, int month, string reason, CancellationToken ct)
    {
        var period = await periodRepo.GetByYearMonthAsync(year, month, ct);
        if (period is null || period.Status != AccountingPeriodStatus.Closed)
            return ServiceResult<AccountingPeriodDto>.Fail(ServiceError.AccountingPeriodNotClosed);

        period.Status = AccountingPeriodStatus.Open;
        period.ReopenedAt = DateTime.UtcNow;
        period.ReopenReason = reason;

        // ClosedAt/ClosedBy are deliberately left as-is - they're the historical record of when/
        // by whom the period was originally closed, not overwritten by a reopen (doc 10 §1's
        // "reissued" framing implies the original close still happened, it's just being revisited).
        await uow.SaveChangesAsync(ct);

        return ServiceResult<AccountingPeriodDto>.Ok(ToDto(period));
    }

    private static AccountingPeriodDto ToDto(AccountingPeriod p) => new(
        p.AccountingPeriodId, p.Year, p.Month, p.Status.ToString(), p.ClosedAt, p.ClosedBy, p.ReopenedAt, p.ReopenReason);
}
