using System.Linq.Expressions;
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IPriceRepository
{
    /// <summary>Tracked - the row currently active (ValidTo == null) for this exact
    /// combination, if any. Used by SetPriceAsync to close it out.</summary>
    Task<Price?> GetActiveAsync(int priceListId, int productId, int? gradeId, int? packSizeId, CancellationToken ct);

    Task<TResult?> GetCurrentAsync<TResult>(int priceListId, int productId, int? gradeId, int? packSizeId,
        Expression<Func<Price, TResult>> selector, CancellationToken ct);

    /// <summary>All rows for this combination, ordered by ValidFrom descending.</summary>
    Task<List<TResult>> GetHistoryAsync<TResult>(int priceListId, int productId, int? gradeId, int? packSizeId,
        Expression<Func<Price, TResult>> selector, CancellationToken ct);

    Task AddAsync(Price price, CancellationToken ct);
}
