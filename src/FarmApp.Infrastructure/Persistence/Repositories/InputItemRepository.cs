using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class InputItemRepository(FarmAppDbContext db) : IInputItemRepository
{
    public Task<InputItem?> GetByIdAsync(int id, CancellationToken ct)
        => db.InputItems.FirstOrDefaultAsync(x => x.InputItemId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<InputItem, TResult>> selector, CancellationToken ct)
        => db.InputItems.AsNoTracking()
            .Where(x => x.InputItemId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<InputItem, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.InputItems.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        => db.InputItems.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.InputItemId != excludeId), ct);

    public async Task AddAsync(InputItem inputItem, CancellationToken ct)
        => await db.InputItems.AddAsync(inputItem, ct);
}
