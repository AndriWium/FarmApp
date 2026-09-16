using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class TillSessionRepository(FarmAppDbContext db) : ITillSessionRepository
{
    public Task<TillSession?> GetByIdAsync(int id, CancellationToken ct)
        => db.TillSessions.FirstOrDefaultAsync(x => x.TillSessionId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<TillSession, TResult>> selector, CancellationToken ct)
        => db.TillSessions.AsNoTracking()
            .Where(x => x.TillSessionId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<bool> HasOpenSessionAsync(int locationId, CancellationToken ct)
        => db.TillSessions.AsNoTracking()
            .AnyAsync(x => x.LocationId == locationId && x.ClosedAt == null, ct);

    public Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<TillSession, TResult>> selector, int? locationId, bool openOnly, CancellationToken ct)
        => db.TillSessions.AsNoTracking()
            .Where(x => (locationId == null || x.LocationId == locationId) && (!openOnly || x.ClosedAt == null))
            .OrderByDescending(x => x.OpenedAt)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(TillSession session, CancellationToken ct)
        => await db.TillSessions.AddAsync(session, ct);

    public Task<List<TillSession>> GetOpenSessionsOpenedInPeriodAsync(int year, int month, CancellationToken ct)
    {
        var periodStart = new DateTime(year, month, 1);
        var periodEnd = periodStart.AddMonths(1);
        return db.TillSessions.AsNoTracking()
            .Where(x => x.ClosedAt == null && x.OpenedAt >= periodStart && x.OpenedAt < periodEnd)
            .ToListAsync(ct);
    }
}
