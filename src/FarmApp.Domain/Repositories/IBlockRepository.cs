using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IBlockRepository
{
    Task<Block?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Block, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Block, TResult>> selector, CancellationToken ct);
    Task AddAsync(Block block, CancellationToken ct);
    void Remove(Block block);
}
