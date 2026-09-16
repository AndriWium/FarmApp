using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

/// <summary>Backs the season-end true-up record (doc 09). Deliberately no top-level CRUD surface
/// beyond read + create - a summary is only ever created by
/// ISeasonCostingService.ConfirmCloseAsync and, per the snapshot discipline the rest of the
/// costing model follows, never updated or deleted afterwards.</summary>
public interface ISeasonCostSummaryRepository
{
    Task<SeasonCostSummary?> GetBySeasonIdAsync(int seasonId, CancellationToken ct);
    Task AddAsync(SeasonCostSummary summary, CancellationToken ct);
}
