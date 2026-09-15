using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(int id, CancellationToken ct);
    Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Sale, TResult>> selector, CancellationToken ct);

    /// <summary>The idempotency lookup (doc 08): CreateSaleAsync calls this first, before
    /// anything else, and returns the existing sale unchanged if found.</summary>
    Task<Sale?> GetByClientGuidAsync(Guid clientGuid, CancellationToken ct);

    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<Sale, TResult>> selector, int? tillSessionId, CancellationToken ct);

    Task AddAsync(Sale sale, CancellationToken ct);
}
