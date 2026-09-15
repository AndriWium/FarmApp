namespace FarmApp.Domain.Repositories;

/// <summary>One chemical-category spray applied under some Season/Planting on a block - the
/// Planting -> Season -> Activity -> ActivityInput -> InputItem trace (doc 05 §5, Phase 3b task
/// brief). Backs IWithholdingLockService, which turns a list of these into
/// FarmApp.Domain.Services.SprayWithholding for the pure lock calculator.</summary>
public record ChemicalSprayRow(int ActivityId, DateTime SprayDate, string ChemicalName, int WithholdingDays);
