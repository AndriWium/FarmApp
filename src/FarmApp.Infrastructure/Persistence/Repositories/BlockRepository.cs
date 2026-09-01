using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class BlockRepository(FarmAppDbContext db) : IBlockRepository
{
    public Task<Block?> GetByIdAsync(int id, CancellationToken ct)
        => db.Blocks.FirstOrDefaultAsync(x => x.BlockId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Block, TResult>> selector, CancellationToken ct)
        => db.Blocks.AsNoTracking()
            .Where(x => x.BlockId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Block, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.Blocks.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        => db.Blocks.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.BlockId != excludeId), ct);

    public async Task AddAsync(Block block, CancellationToken ct)
        => await db.Blocks.AddAsync(block, ct);
}
