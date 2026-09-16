namespace FarmApp.Domain.Entities;

/// <summary>Simple master data (Fuel, Repairs, Wages, Rates, Packaging, ...) - Grade-style
/// pattern: unique Name, soft-delete via IsActive (doc 02, Phase 4b task brief).
/// IsFarmingDirect distinguishes a farming-direct cost (fits into a season's true-up/costing,
/// doc 09) from a general business overhead (rent, admin, bakkie fuel for market runs) - doc 04
/// §1's income statement groups expenses by category, and doc 09's per-season costing wants to
/// know which categories are even eligible to be allocated to a season in the first place. Not
/// enforced anywhere yet this phase (Expense.SeasonId is optional and unrestricted) - a flag for
/// future farming-report/costing work to filter on.</summary>
public class ExpenseCategory
{
    public int ExpenseCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsFarmingDirect { get; set; }
    public bool IsActive { get; set; } = true;
}
