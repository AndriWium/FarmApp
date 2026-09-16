using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.RoadmapItems;

public record RoadmapItemDto(
    int RoadmapItemId,
    string Title,
    string Description,
    RoadmapItemStatus Status,
    int SortOrder,
    string? TargetPhase,
    DateTime? CompletedOn
    );

public record CreateRoadmapItemRequest(
    string Title,
    string Description,
    RoadmapItemStatus Status,
    int SortOrder,
    string? TargetPhase
    );

// CompletedOn is deliberately absent from both requests - it's derived, not client-supplied (see
// RoadmapItemService.UpdateAsync's remarks): it's auto-set the moment Status flips to Done, and
// cleared if a Done item is ever moved back off Done. A client can't backdate or fabricate it.
public record UpdateRoadmapItemRequest(
    string Title,
    string Description,
    RoadmapItemStatus Status,
    int SortOrder,
    string? TargetPhase
    );
