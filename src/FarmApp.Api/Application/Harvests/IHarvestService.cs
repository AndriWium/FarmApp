using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Harvests;

public interface IHarvestService
{
    Task<List<HarvestDto>> GetAllAsync(int? seasonId, CancellationToken ct);
    Task<HarvestDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<HarvestDto>> CreateHarvestAsync(CreateHarvestRequest request, CancellationToken ct);
}
