using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Grades;

public interface IGradeService
{
    Task<List<GradeDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<GradeDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<GradeDto>> CreateAsync(CreateGradeRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateGradeRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
