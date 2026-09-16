using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ISeasonRepository
{
    Task<Season?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Season, TResult>> selector, CancellationToken ct);

    /// <summary>Optional plantingId filter - GET /api/v1/seasons?plantingId=.</summary>
    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Season, TResult>> selector, int? plantingId, CancellationToken ct);

    Task AddAsync(Season season, CancellationToken ct);

    /// <summary>Every Season whose [StartDate, EndDate] window overlaps the given [year, month] -
    /// the month-end close checklist's season-review item (doc 10 §1 item 4). The checklist
    /// service itself decides what to flag from the result: an Open season with no
    /// EstimatedCostPerKg yet ("open seasons reviewed"), or a Closed season whose EndDate actually
    /// falls in this period with no matching SeasonCostSummary ("closed seasons true-up
    /// posted").</summary>
    Task<List<Season>> GetSeasonsOverlappingPeriodAsync(int year, int month, CancellationToken ct);
}
