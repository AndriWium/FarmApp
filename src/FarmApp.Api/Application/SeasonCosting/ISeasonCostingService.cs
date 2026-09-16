using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.SeasonCosting;

/// <summary>Doc 09's season-end true-up, as a two-step confirmable operation: PreviewCloseAsync
/// computes and returns the numbers without writing anything; ConfirmCloseAsync is the
/// user-confirmed posting ("show the number, owner clicks approve") that actually persists the
/// SeasonCostSummary and closes the season. Never a single CloseSeasonAsync - silent revaluations
/// destroy trust in reports (doc 09).</summary>
public interface ISeasonCostingService
{
    Task<ServiceResult<SeasonCostPreviewDto>> PreviewCloseAsync(int seasonId, CancellationToken ct);
    Task<ServiceResult<SeasonCostSummaryDto>> ConfirmCloseAsync(int seasonId, CancellationToken ct);
}
