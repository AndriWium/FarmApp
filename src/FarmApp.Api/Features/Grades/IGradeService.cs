namespace FarmApp.Api.Features.Grades;

public interface IGradeService
{
    Task<List<GradeDto>> GetAllAsync(CancellationToken ct);
    Task<GradeDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<GradeDto> CreateAsync(CreateGradeRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, CreateGradeRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
