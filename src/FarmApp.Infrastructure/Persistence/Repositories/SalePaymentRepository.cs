using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class SalePaymentRepository(FarmAppDbContext db) : ISalePaymentRepository
{
    public Task<List<TResult>> GetBySaleIdAsync<TResult>(
        int saleId, Expression<Func<SalePayment, TResult>> selector, CancellationToken ct)
        => db.SalePayments.AsNoTracking()
            .Where(x => x.SaleId == saleId)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<SalePayment> payments, CancellationToken ct)
        => await db.SalePayments.AddRangeAsync(payments, ct);

    public async Task<decimal> GetCardTotalForTillSessionAsync(int tillSessionId, CancellationToken ct)
        => await (
            from p in db.SalePayments.AsNoTracking()
            join s in db.Sales.AsNoTracking() on p.SaleId equals s.SaleId
            where s.TillSessionId == tillSessionId
                && s.Status == SaleStatus.Complete
                && p.Method == SalePaymentMethod.Card
            select p.Amount
        ).SumAsync(x => (decimal?)x, ct) ?? 0m;

    public async Task<decimal> GetAccountTotalForCustomerAsync(int customerId, CancellationToken ct)
        => await (
            from p in db.SalePayments.AsNoTracking()
            join s in db.Sales.AsNoTracking() on p.SaleId equals s.SaleId
            where s.CustomerId == customerId
                && s.Status == SaleStatus.Complete
                && p.Method == SalePaymentMethod.Account
            select p.Amount
        ).SumAsync(x => (decimal?)x, ct) ?? 0m;
}
