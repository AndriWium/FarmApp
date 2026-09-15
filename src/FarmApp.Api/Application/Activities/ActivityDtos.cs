namespace FarmApp.Api.Application.Activities;

public record ActivityInputDto(int ActivityInputId, int InputItemId, decimal Qty, decimal UnitCost);

public record ActivityDto(
    int ActivityId, int SeasonId, int ActivityTypeId, DateTime Date,
    decimal LabourHours, decimal LabourCost, string? Notes, List<ActivityInputDto> Inputs);

/// <summary>UnitCost is deliberately absent - it's never client-supplied, always snapshotted
/// server-side from IInputStockMovementRepository.GetWeightedAverageCostAsync at the moment of
/// consumption (doc 02, task brief).</summary>
public record CreateActivityInputLineRequest(int InputItemId, decimal Qty);

public record CreateActivityRequest(
    int SeasonId, int ActivityTypeId, DateTime Date, decimal LabourHours, decimal LabourCost,
    string? Notes, List<CreateActivityInputLineRequest> Inputs);
