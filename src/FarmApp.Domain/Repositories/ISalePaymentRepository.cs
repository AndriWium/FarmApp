using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ISalePaymentRepository
{
    Task<List<TResult>> GetBySaleIdAsync<TResult>(
        int saleId, Expression<Func<SalePayment, TResult>> selector, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<SalePayment> payments, CancellationToken ct);
}
