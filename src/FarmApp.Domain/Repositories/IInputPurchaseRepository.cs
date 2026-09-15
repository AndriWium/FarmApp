using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IInputPurchaseRepository
{
    Task<InputPurchase?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<InputPurchase, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<InputPurchase, TResult>> selector, CancellationToken ct);
    Task AddAsync(InputPurchase purchase, CancellationToken ct);
}
