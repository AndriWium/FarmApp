using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IProducePurchaseRepository
{
    Task<ProducePurchase?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<ProducePurchase, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<ProducePurchase, TResult>> selector, CancellationToken ct);
    Task AddAsync(ProducePurchase purchase, CancellationToken ct);
}
