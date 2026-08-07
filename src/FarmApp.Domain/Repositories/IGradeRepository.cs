using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IGradeRepository
{
    Task<Grade?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Grade>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Grade grade, CancellationToken ct);
    void Remove(Grade grade);
}