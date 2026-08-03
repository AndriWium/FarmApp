using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IBlockRepository
{
    Task<Block?> GetById(int id, CancellationToken ct);
    Task<List<Block>> GetAll(CancellationToken ct);
    Task Add(Block block, CancellationToken ct);
    void Remove(Block block);
}
