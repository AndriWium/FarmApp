using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ICropRepository
{
    Task<Crop?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Crop, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Crop, TResult>> selector, CancellationToken ct);
    Task AddAsync(Crop crop, CancellationToken ct);
    void Remove(Crop crop);
}
