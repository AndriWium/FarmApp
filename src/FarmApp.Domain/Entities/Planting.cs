using FarmApp.Domain.Enums;

namespace FarmApp.Domain.Entities;

/// <summary>A crop instance on a block: BlockId/CultivarId are cross-repo-validated FKs (same
/// shape as Cultivar->Crop, Phase 0b-1 precedent). EndDate is nullable - a planting doesn't
/// "deactivate" (no IsActive column, deliberately - doc 02/task brief), it just ends, and
/// EndDate NULL already captures "still growing".</summary>
public class Planting
{
    public int PlantingId { get; set; }
    public int BlockId { get; set; } // plain FK column, no navigation
    public int CultivarId { get; set; } // plain FK column, no navigation
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PlantingType Type { get; set; }
    public int? PlantCount { get; set; }
    public string? Notes { get; set; }
}
