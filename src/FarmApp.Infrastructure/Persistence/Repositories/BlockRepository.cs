using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class BlockRepository(FarmAppDbContext db) : IBlockRepository
{
    public Task<Block?> GetByIdAsync(int id, CancellationToken ct)
    {
        return db.Blocks.FirstOrDefaultAsync(x => x.BlockId == id, ct);
    }

    public Task<List<Block>> GetAllAsync(CancellationToken ct)
    {
        return db.Blocks.AsNoTracking().ToListAsync(ct);
    } 

    public async Task AddAsync(Block block, CancellationToken ct)
    { 
        await db.Blocks.AddAsync(block, ct);
    }

    public void Remove(Block block)
    {
        db.Blocks.Remove(block);
    }
}
