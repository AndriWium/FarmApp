using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Grades;

public class GradeService(IGradeRepository repo, IUnitOfWork uow) : IGradeService
{
    public Task<List<GradeDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(g => new GradeDto(g.GradeId, g.Name, g.IsActive), includeInactive, ct);

    public Task<GradeDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, g => new GradeDto(g.GradeId, g.Name, g.IsActive), ct);

    public async Task<ServiceResult<GradeDto>> CreateAsync(CreateGradeRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByNameAsync(request.Name, excludeId: null, ct))
            return ServiceResult<GradeDto>.Fail(ServiceError.DuplicateName);

        var grade = new Grade { Name = request.Name };
        await repo.AddAsync(grade, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<GradeDto>.Ok(new GradeDto(grade.GradeId, grade.Name, grade.IsActive));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateGradeRequest request, CancellationToken ct)
    {
        var grade = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (grade is null) return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        grade.Name = request.Name;
        grade.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var grade = await repo.GetByIdAsync(id, ct);
        if (grade is null) return ServiceError.NotFound;

        grade.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
