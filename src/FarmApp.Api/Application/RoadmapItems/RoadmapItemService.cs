using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.RoadmapItems;

public class RoadmapItemService(IRoadmapItemRepository repo, IUnitOfWork uow) : IRoadmapItemService
{
    public Task<List<RoadmapItemDto>> GetAllAsync(CancellationToken ct)
        => repo.GetAllAsync(ToDtoExpression, ct);

    public Task<RoadmapItemDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDtoExpression, ct);

    public async Task<RoadmapItemDto> CreateAsync(CreateRoadmapItemRequest request, CancellationToken ct)
    {
        var item = new RoadmapItem
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            SortOrder = request.SortOrder,
            TargetPhase = request.TargetPhase,
            // A brand-new item seeded straight in as Done (unusual, but not invalid - e.g.
            // backfilling something already shipped) still gets its CompletedOn stamped, matching
            // UpdateAsync's own rule below.
            CompletedOn = request.Status == RoadmapItemStatus.Done ? DateTime.UtcNow : null,
        };
        await repo.AddAsync(item, ct);
        await uow.SaveChangesAsync(ct);
        return ToDto(item);
    }

    public async Task<bool> UpdateAsync(int id, UpdateRoadmapItemRequest request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(id, ct);
        if (item is null) return false;

        // CompletedOn is server-derived, not client-supplied (judgment call - see DECISIONS.md):
        // it's stamped the moment Status transitions *into* Done, and cleared the moment it
        // transitions *out* of Done (e.g. a mistaken mark-done gets reopened). Re-saving while
        // already Done leaves the original CompletedOn untouched.
        if (request.Status == RoadmapItemStatus.Done && item.Status != RoadmapItemStatus.Done)
            item.CompletedOn = DateTime.UtcNow;
        else if (request.Status != RoadmapItemStatus.Done)
            item.CompletedOn = null;

        item.Title = request.Title;
        item.Description = request.Description;
        item.Status = request.Status;
        item.SortOrder = request.SortOrder;
        item.TargetPhase = request.TargetPhase;

        await uow.SaveChangesAsync(ct);
        return true;
    }

    private static readonly System.Linq.Expressions.Expression<Func<RoadmapItem, RoadmapItemDto>> ToDtoExpression =
        x => new RoadmapItemDto(x.RoadmapItemId, x.Title, x.Description, x.Status, x.SortOrder, x.TargetPhase, x.CompletedOn);

    private static RoadmapItemDto ToDto(RoadmapItem x) =>
        new(x.RoadmapItemId, x.Title, x.Description, x.Status, x.SortOrder, x.TargetPhase, x.CompletedOn);
}
