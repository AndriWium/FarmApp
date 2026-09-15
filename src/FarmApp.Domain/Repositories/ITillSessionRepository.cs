using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ITillSessionRepository
{
    Task<TillSession?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<TillSession, TResult>> selector, CancellationToken ct);

    /// <summary>True if this location already has an open (ClosedAt == null) session - the
    /// one-open-session-per-location rule OpenAsync enforces.</summary>
    Task<bool> HasOpenSessionAsync(int locationId, CancellationToken ct);

    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<TillSession, TResult>> selector, int? locationId, bool openOnly, CancellationToken ct);

    Task AddAsync(TillSession session, CancellationToken ct);
}
