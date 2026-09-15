using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Seasons;

public interface ISeasonService
{
    Task<List<SeasonDto>> GetAllAsync(int? plantingId, CancellationToken ct);
    Task<SeasonDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<SeasonDto>> CreateAsync(CreateSeasonRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateSeasonRequest request, CancellationToken ct);
}
