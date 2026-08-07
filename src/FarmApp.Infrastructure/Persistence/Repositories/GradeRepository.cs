using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class GradeRepository (FarmAppDbContext db) : IGradeRepository
{
    public Task<Grade?> GetByIdAsync(int id, CancellationToken ct)
    {
        return db.Grades.FirstOrDefaultAsync(x => x.GradeId == id, ct);
    }

    public Task<List<Grade>> GetAllAsync(CancellationToken ct)
    {
        return db.Grades.AsNoTracking().ToListAsync(ct);
    } 

    public async Task AddAsync(Grade grade, CancellationToken ct)
    { 
        await db.Grades.AddAsync(grade, ct);
    }

    public void Remove(Grade grade)
    {
        db.Grades.Remove(grade);
    }
}
