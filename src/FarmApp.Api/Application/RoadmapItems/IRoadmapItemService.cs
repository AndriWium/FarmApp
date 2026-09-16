namespace FarmApp.Api.Application.RoadmapItems;

public interface IRoadmapItemService
{
    Task<List<RoadmapItemDto>> GetAllAsync(CancellationToken ct);
    Task<RoadmapItemDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<RoadmapItemDto> CreateAsync(CreateRoadmapItemRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateRoadmapItemRequest request, CancellationToken ct);
}
