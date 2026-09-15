namespace FarmApp.Domain.Services;

/// <summary>One spray event that matters for a block's withholding lock - a chemical-category
/// ActivityInput's parent Activity date, plus the chemical's WithholdingDays (doc 05 §5). Not an
/// EF entity - a plain projection the repository query builds (Planting -> Season -> Activity ->
/// ActivityInput -> InputItem, task brief) and IWithholdingLockCalculator consumes, same shape as
/// BatchAvailability/CostedAllocation.</summary>
public record SprayWithholding(int ActivityId, DateTime SprayDate, string ChemicalName, int WithholdingDays)
{
    /// <summary>The date produce sprayed on SprayDate may next be harvested - "block locked until
    /// DATE" (doc 05 §5).</summary>
    public DateTime LockedUntil => SprayDate.AddDays(WithholdingDays);
}
