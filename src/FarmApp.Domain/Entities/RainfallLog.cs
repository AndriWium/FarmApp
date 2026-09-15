namespace FarmApp.Domain.Entities;

/// <summary>One rainfall reading per day (doc 01/02). Doc 02 shows no surrogate id for this
/// table (just Date/Mm/Notes), but a surrogate int RainfallLogId is used here instead, with a
/// unique index on Date enforcing the "one reading per day" natural-key rule at the DB level -
/// consistency with every other entity in the codebase (all use an int surrogate key, all
/// repository/controller plumbing is GetByIdAsync(int id)-shaped); see DECISIONS.md. Date is
/// DateOnly, not DateTime - a rainfall reading belongs to a day, not a timestamp (doc 11 "no
/// datetimes where a date will do"), same precedent as Price.ValidFrom/ValidTo.</summary>
public class RainfallLog
{
    public int RainfallLogId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Mm { get; set; } // decimal(18,3) - a quantity, not money
    public string? Notes { get; set; }
}
