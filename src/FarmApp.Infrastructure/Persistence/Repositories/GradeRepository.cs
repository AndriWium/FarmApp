using System.Linq.Expressions;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class GradeRepository(FarmAppDbContext db) : IGradeRepository
{
    public Task<Grade?> GetByIdAsync(int id, CancellationToken ct)
        => db.Grades.FirstOrDefaultAsync(x => x.GradeId == id, ct);

    public Task<TResult?> GetByIdAsync<TResult>(int id, Expression<Func<Grade, TResult>> selector, CancellationToken ct)
        => db.Grades.AsNoTracking()
            .Where(x => x.GradeId == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct);

    public Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<Grade, TResult>> selector, bool includeInactive, CancellationToken ct)
        => db.Grades.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(selector)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        => db.Grades.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.GradeId != excludeId), ct);

    public async Task AddAsync(Grade grade, CancellationToken ct)
        => await db.Grades.AddAsync(grade, ct);
}
