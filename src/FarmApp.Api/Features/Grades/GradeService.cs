using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Features.Grades;

public class GradeService(IGradeRepository repo, IUnitOfWork uow) : IGradeService
{
    public Task<List<GradeDto>> GetAllAsync(CancellationToken ct)
        => repo.GetAllAsync(g => new GradeDto(g.GradeId, g.Name), ct);

    public Task<GradeDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, g => new GradeDto(g.GradeId, g.Name), ct);

    public async Task<GradeDto> CreateAsync(CreateGradeRequest request, CancellationToken ct)
    {
        var grade = new Grade { Name = request.Name };
        await repo.AddAsync(grade, ct);
        await uow.SaveChangesAsync(ct);
        return new GradeDto(grade.GradeId, grade.Name);
    }

    public async Task<bool> UpdateAsync(int id, CreateGradeRequest request, CancellationToken ct)
    {
        var grade = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (grade is null) return false;

        grade.Name = request.Name;
        await uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var grade = await repo.GetByIdAsync(id, ct);   // tracked entity — required to remove
        if (grade is null) return false;

        repo.Remove(grade);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
