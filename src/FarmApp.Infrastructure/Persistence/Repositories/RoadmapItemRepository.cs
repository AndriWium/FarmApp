using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class RoadmapItemRepository(FarmAppDbContext db) : IRoadmapItemRepository
{
    public Task<RoadmapItem?> GetByIdAsync(int id, CancellationToken ct)
        => db.RoadmapItems.FirstOrDefaultAsync(x => x.RoadmapItemId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<RoadmapItem, TResult>> selector, CancellationToken ct)
        => db.RoadmapItems.AsNoTracking()
            .Where(x => x.RoadmapItemId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    // Sorted by SortOrder (drag-order within a status column) then Title, so an unsorted set of
    // freshly-seeded rows still reads sensibly before anyone has reordered anything.
    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<RoadmapItem, TResult>> selector, CancellationToken ct)
        => db.RoadmapItems.AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(CancellationToken ct)
        => db.RoadmapItems.AsNoTracking().AnyAsync(ct);

    public async Task AddAsync(RoadmapItem item, CancellationToken ct)
        => await db.RoadmapItems.AddAsync(item, ct);
}
