namespace FarmApp.Domain.Entities;

/// <summary>Header for a physical stock count against a set of existing StockBatch rows. A
/// stock take never creates new batches - every StockTakeLine counts a batch that already exists
/// (received earlier via harvest-in or purchase-in), it only reconciles that batch's recorded
/// on-hand against what was physically counted (doc 02).</summary>
public class StockTake
{
    public int StockTakeId { get; set; }
    public DateTime Date { get; set; }
    public int? LocationId { get; set; } // plain FK column, no navigation - matches StockMovement.LocationId
    public string? Notes { get; set; }
}
