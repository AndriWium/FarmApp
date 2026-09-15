using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IPriceListRepository
{
    Task<PriceList?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<PriceList, TResult>> selector, CancellationToken ct);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<PriceList, TResult>> selector, bool includeInactive, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct);
    Task AddAsync(PriceList priceList, CancellationToken ct);
}
