using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IStockTakeRepository
{
    Task<StockTake?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<StockTake, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<StockTake, TResult>> selector, CancellationToken ct);
    Task AddAsync(StockTake stockTake, CancellationToken ct);
}
