using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class PlantingRepository(FarmAppDbContext db) : IPlantingRepository
{
    public Task<Planting?> GetByIdAsync(int id, CancellationToken ct)
        => db.Plantings.FirstOrDefaultAsync(x => x.PlantingId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Planting, TResult>> selector, CancellationToken ct)
        => db.Plantings.AsNoTracking()
            .Where(x => x.PlantingId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Planting, TResult>> selector, int? blockId, CancellationToken ct)
        => db.Plantings.AsNoTracking()
            .Where(x => blockId == null || x.BlockId == blockId)
            .OrderByDescending(x => x.StartDate)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Planting planting, CancellationToken ct)
        => await db.Plantings.AddAsync(planting, ct);
}
