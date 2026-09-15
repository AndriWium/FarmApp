using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ISalePaymentRepository
{
    Task<List<TResult>> GetBySaleIdAsync<TResult>(
        int saleId, Expression<Func<SalePayment, TResult>> selector, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<SalePayment> payments, CancellationToken ct);

    /// <summary>SUM(Amount) of every Card-method SalePayment belonging to a Complete Sale in this
    /// TillSession - TillSessionService.CloseAsync's SystemCardTotal (doc 02/01 §4's day-close).
    /// Refunded sales are excluded: their card payment shouldn't count toward the day's system
    /// total even though the physical card-machine batch total (external to this system) already
    /// includes whatever was charged and later refunded on the machine itself.</summary>
    Task<decimal> GetCardTotalForTillSessionAsync(int tillSessionId, CancellationToken ct);

    /// <summary>SUM(Amount) of every Account-method SalePayment belonging to a Complete Sale for
    /// this customer - the "owed" side of CustomerPaymentService.GetBalanceAsync's derived balance
    /// (doc 02: a customer's balance is never stored, always derived). Only the Account-method
    /// portion of a sale counts, never the sale's full total (a sale can split Card+Account), and
    /// Refunded sales are excluded entirely - a refunded sale's debt shouldn't count.</summary>
    Task<decimal> GetAccountTotalForCustomerAsync(int customerId, CancellationToken ct);
}
