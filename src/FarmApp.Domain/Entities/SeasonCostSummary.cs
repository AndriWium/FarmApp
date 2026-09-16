namespace FarmApp.Domain.Entities;

/// <summary>The season-end true-up record (doc 09/02) - created once, by
/// ISeasonCostingService.ConfirmCloseAsync, from the season's actual accumulated costs/yield.
/// Never recreated or edited after that: SeasonId is unique (one summary per season), matching
/// the "historical rows never change" snapshot discipline the rest of the costing model follows
/// (StockBatch.UnitCost, SaleLine.CostAtSale). OverheadAllocated is always 0 for now (doc 09/11:
/// "start by not allocating") - the column exists per doc 02's own field list for the future
/// refinement doc 09 calls out, not because anything computes into it yet. TrueUpAmount is the
/// one labelled adjustment figure doc 09 describes ("costing true-up") - kept as a field here
/// rather than a separate ledger entity, since it's a single number produced 1:1 with the summary
/// that computed it (see DECISIONS.md).</summary>
public class SeasonCostSummary
{
    public int SeasonCostSummaryId { get; set; }
    public int SeasonId { get; set; } // plain FK column, no navigation; unique - one summary per season

    public decimal InputCost { get; set; } // decimal(18,2)
    public decimal LabourCost { get; set; } // decimal(18,2)
    public decimal OverheadAllocated { get; set; } // decimal(18,2) - always 0 for now
    public decimal TotalKgHarvested { get; set; } // decimal(18,3)
    public decimal CostPerKg { get; set; } // decimal(18,2) - the actual, true-up cost per kg

    /// <summary>Σ(CostPerKg - batch.EstimatedUnitCost) x batch.QtyKg across every StockBatch this
    /// season's harvests produced (per batch, so a mid-season estimate change is honoured exactly
    /// rather than approximated by one blended estimate). Negative means sales were overcosted
    /// during the season (COS overstated) and this is a credit; positive means undercosted.</summary>
    public decimal TrueUpAmount { get; set; } // decimal(18,2)

    public DateTime ClosedAt { get; set; }
}
