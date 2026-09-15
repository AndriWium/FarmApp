using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class CultivarRepository(FarmAppDbContext db) : ICultivarRepository
{
    public Task<Cultivar?> GetByIdAsync(int id, CancellationToken ct)
        => db.Cultivars.FirstOrDefaultAsync(x => x.CultivarId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Cultivar, TResult>> selector, CancellationToken ct)
        => db.Cultivars.AsNoTracking()
            .Where(x => x.CultivarId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Cultivar, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.Cultivars.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(int cropId, string name, int? excludeId, CancellationToken ct)
        => db.Cultivars.AsNoTracking()
            .AnyAsync(x => x.CropId == cropId && x.Name == name && (excludeId == null || x.CultivarId != excludeId), ct);

    public async Task AddAsync(Cultivar cultivar, CancellationToken ct)
        => await db.Cultivars.AddAsync(cultivar, ct);
}
