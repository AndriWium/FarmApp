namespace FarmApp.Domain.Entities;

/// <summary>One cashier's open-to-close session at a location. Every Sale must reference an open
/// TillSession (doc 02) - a sale can't happen against a closed till. Day-close fields
/// (SystemCardTotal/CardMachineBatchTotal/Difference/DifferenceNote) are Phase 2b scope (task
/// brief) - present now per doc 02's field list so no later migration is needed, but nothing
/// writes them yet; OpenAsync is this phase's only write path.</summary>
public class TillSession
{
    public int TillSessionId { get; set; }
    public int LocationId { get; set; } // plain FK column, no navigation (matches Block/Crop/Cultivar precedent)
    public DateTime OpenedAt { get; set; }
    public int OpenedBy { get; set; } // plain FK column to AppUser, no navigation - derived from the JWT claims, never client-supplied
    public DateTime? ClosedAt { get; set; }

    public decimal? SystemCardTotal { get; set; } // decimal(18,2) - Phase 2b day close
    public decimal? CardMachineBatchTotal { get; set; } // decimal(18,2) - Phase 2b day close
    public decimal? Difference { get; set; } // decimal(18,2) - Phase 2b day close
    public string? DifferenceNote { get; set; } // Phase 2b day close
}
