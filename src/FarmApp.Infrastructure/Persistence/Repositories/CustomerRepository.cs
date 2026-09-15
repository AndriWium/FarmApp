using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class CustomerRepository(FarmAppDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(int id, CancellationToken ct)
        => db.Customers.FirstOrDefaultAsync(x => x.CustomerId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Customer, TResult>> selector, CancellationToken ct)
        => db.Customers.AsNoTracking()
            .Where(x => x.CustomerId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Customer, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.Customers.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public async Task AddAsync(Customer customer, CancellationToken ct)
        => await db.Customers.AddAsync(customer, ct);
}
