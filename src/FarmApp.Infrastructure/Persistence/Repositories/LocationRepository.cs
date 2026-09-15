using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class LocationRepository(FarmAppDbContext db) : ILocationRepository
{
    public Task<Location?> GetByIdAsync(int id, CancellationToken ct)
        => db.Locations.FirstOrDefaultAsync(x => x.LocationId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Location, TResult>> selector, CancellationToken ct)
        => db.Locations.AsNoTracking()
            .Where(x => x.LocationId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Location, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.Locations.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        => db.Locations.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.LocationId != excludeId), ct);

    public async Task AddAsync(Location location, CancellationToken ct)
        => await db.Locations.AddAsync(location, ct);
}
