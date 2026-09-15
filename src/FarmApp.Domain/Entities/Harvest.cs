namespace FarmApp.Domain.Entities;

/// <summary>Header for a harvest event on a Season - one picking session, N lines. Each line
/// creates its own StockBatch (Source: Harvest, HarvestId populated) + seeding HarvestIn
/// movement (see HarvestLine), the same header+lines shape as ProducePurchase/ProducePurchaseLine
/// (doc 02, Phase 3b task brief). PickedBy is left as an optional caller-supplied field rather
/// than defaulted from the authenticated user (TillSession.OpenedBy's precedent) - the person
/// physically picking is often not the person logging the harvest afterwards on a phone (see
/// DECISIONS.md). WithholdingOverrideReason is null unless the harvest date fell inside a
/// chemical withholding-period lock on this season's block and a human explicitly overrode it
/// (doc 05 §5) - a dedicated queryable column, not folded into Notes, so "why was this
/// overridden" survives as normal, findable data rather than buried audit-log text (task
/// brief).</summary>
public class Harvest : IPeriodLocked
{
    public int HarvestId { get; set; }
    public int SeasonId { get; set; } // plain FK column, no navigation
    public DateTime Date { get; set; }
    public int? PickedBy { get; set; } // nullable FK to AppUser, no navigation - caller-supplied, never derived from the JWT
    public string? Notes { get; set; }
    public string? WithholdingOverrideReason { get; set; }

    DateTime IPeriodLocked.BusinessDate => Date;
}
