using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ICustomerPaymentRepository
{
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<CustomerPayment, TResult>> selector, CancellationToken ct);

    Task<List<TResult>> GetByCustomerIdAsync<TResult>(
        int customerId, Expression<Func<CustomerPayment, TResult>> selector, CancellationToken ct);

    /// <summary>SUM(Amount) of every payment recorded against this customer - the "paid" side of
    /// CustomerPaymentService.GetBalanceAsync's derived balance.</summary>
    Task<decimal> GetTotalByCustomerIdAsync(int customerId, CancellationToken ct);

    Task AddAsync(CustomerPayment payment, CancellationToken ct);
}
