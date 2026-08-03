using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IGradeRepository
{
    Task<Grade?> GetById(int id, CancellationToken ct);
    Task<List<Grade>> GetAll(CancellationToken ct);
    Task Add(Grade grade, CancellationToken ct);
    void Remove(Grade grade);
}