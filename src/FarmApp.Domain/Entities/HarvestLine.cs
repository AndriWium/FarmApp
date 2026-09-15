namespace FarmApp.Domain.Entities;

/// <summary>One product/grade/qty line of a Harvest - creates its own StockBatch (Source:
/// Harvest, HarvestId pointing at the header) + seeding HarvestIn movement, the same shape as
/// ProducePurchaseLine. UnitCost is deliberately not persisted here (not in doc 02's field list
/// for HarvestLine) - it's supplied on the create request and flows straight into the batch it
/// creates, exactly like ProducePurchaseLine's ShelfLifeDays (see DECISIONS.md). It's the
/// season-estimate cost doc 09 describes (an estimate, e.g. last season's number or gut feel),
/// never a real computed cost - the season-end true-up (SeasonCostSummary) is explicitly Phase
/// 4's job.</summary>
public class HarvestLine
{
    public int HarvestLineId { get; set; }
    public int HarvestId { get; set; } // plain FK column, no navigation
    public int ProductId { get; set; } // plain FK column, no navigation
    public int? GradeId { get; set; } // plain FK column, no navigation; grades don't apply to every product

    public decimal QtyKg { get; set; } // decimal(18,3)
}
