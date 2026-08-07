using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IBlockRepository
{
    Task<Block?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Block>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Block block, CancellationToken ct);
    void Remove(Block block);
}
