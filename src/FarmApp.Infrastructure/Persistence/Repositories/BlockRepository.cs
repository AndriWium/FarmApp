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

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Block, TResult>> selector, CancellationToken ct)
        => db.Blocks.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Block block, CancellationToken ct)
        => await db.Blocks.AddAsync(block, ct);

    public void Remove(Block block)
        => db.Blocks.Remove(block);
}
