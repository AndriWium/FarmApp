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

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Crop, TResult>> selector, CancellationToken ct)
        => db.Crops.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Crop crop, CancellationToken ct)
        => await db.Crops.AddAsync(crop, ct);

    public void Remove(Crop crop)
        => db.Crops.Remove(crop);
}
