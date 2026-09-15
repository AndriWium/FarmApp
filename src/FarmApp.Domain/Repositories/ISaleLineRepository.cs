using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ISaleLineRepository
{
    Task<List<TResult>> GetBySaleIdAsync<TResult>(
        int saleId, Expression<Func<SaleLine, TResult>> selector, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<SaleLine> lines, CancellationToken ct);
}
