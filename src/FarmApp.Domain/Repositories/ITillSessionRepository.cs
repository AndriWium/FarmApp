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

    /// <summary>TillSessions opened within the given [year, month] whose ClosedAt is still null -
    /// the month-end close checklist's hard-blocking item (doc 10 §1 item 1, task brief: "a month
    /// cannot close while a till session for it is still open").</summary>
    Task<List<TillSession>> GetOpenSessionsOpenedInPeriodAsync(int year, int month, CancellationToken ct);
}
