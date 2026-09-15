using System.Linq.Expressions;
using FarmApp.Domain.Entities;
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
}
