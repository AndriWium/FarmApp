using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

/// <summary>The "Up & Coming" tab's backing table (AI Guide/14-up-and-coming.md) - deliberately
/// simple master data: Owner-only CRUD, read-only for everyone else. TargetPhase/CompletedOn are
/// nullable - most items are seeded without a firm target phase, and CompletedOn is only set once
/// Status flips to Done.</summary>
public class RoadmapItem
{
    public int RoadmapItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoadmapItemStatus Status { get; set; } = RoadmapItemStatus.Planned;
    public int SortOrder { get; set; }
    public string? TargetPhase { get; set; }
    public DateTime? CompletedOn { get; set; }
}
