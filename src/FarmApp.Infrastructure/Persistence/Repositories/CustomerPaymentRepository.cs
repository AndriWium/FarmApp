using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class CustomerPaymentRepository(FarmAppDbContext db) : ICustomerPaymentRepository
{
    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<CustomerPayment, TResult>> selector, CancellationToken ct)
        => db.CustomerPayments.AsNoTracking()
            .Where(x => x.CustomerPaymentId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetByCustomerIdAsync<TResult>(
        int customerId, Expression<Func<CustomerPayment, TResult>> selector, CancellationToken ct)
        => db.CustomerPayments.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.Date)
            .Select(selector)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalByCustomerIdAsync(int customerId, CancellationToken ct)
        => await db.CustomerPayments.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .SumAsync(x => (decimal?)x.Amount, ct) ?? 0m;

    public async Task AddAsync(CustomerPayment payment, CancellationToken ct)
        => await db.CustomerPayments.AddAsync(payment, ct);
}
