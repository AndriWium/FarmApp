using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class RainfallLogRepository(FarmAppDbContext db) : IRainfallLogRepository
{
    public Task<RainfallLog?> GetByIdAsync(int id, CancellationToken ct)
        => db.RainfallLogs.FirstOrDefaultAsync(x => x.RainfallLogId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<RainfallLog, TResult>> selector, CancellationToken ct)
        => db.RainfallLogs.AsNoTracking()
            .Where(x => x.RainfallLogId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<RainfallLog, TResult>> selector, DateOnly? from, DateOnly? to, CancellationToken ct)
        => db.RainfallLogs.AsNoTracking()
            .Where(x => (from == null || x.Date >= from) && (to == null || x.Date <= to))
            .OrderByDescending(x => x.Date)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByDateAsync(DateOnly date, int? excludeId, CancellationToken ct)
        => db.RainfallLogs.AsNoTracking()
            .AnyAsync(x => x.Date == date && (excludeId == null || x.RainfallLogId != excludeId), ct);

    public async Task AddAsync(RainfallLog rainfallLog, CancellationToken ct)
        => await db.RainfallLogs.AddAsync(rainfallLog, ct);
}
