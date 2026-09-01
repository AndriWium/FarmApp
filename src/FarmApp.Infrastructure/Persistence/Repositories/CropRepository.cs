using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class CropRepository(FarmAppDbContext db) : ICropRepository
{
    public Task<Crop?> GetByIdAsync(int id, CancellationToken ct)
        => db.Crops.FirstOrDefaultAsync(x => x.CropId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Crop, TResult>> selector, CancellationToken ct)
        => db.Crops.AsNoTracking()
            .Where(x => x.CropId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Crop, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.Crops.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        => db.Crops.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.CropId != excludeId), ct);

    public async Task AddAsync(Crop crop, CancellationToken ct)
        => await db.Crops.AddAsync(crop, ct);
}
