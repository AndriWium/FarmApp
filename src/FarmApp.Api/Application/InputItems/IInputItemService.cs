using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.InputItems;

public interface IInputItemService
{
    Task<List<InputItemDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<InputItemDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<InputItemDto>> CreateAsync(CreateInputItemRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateInputItemRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);

    /// <summary>SUM(Qty) over this item's InputStockMovement ledger. Null means the item itself
    /// doesn't exist (NotFound) - 0 is a legitimate on-hand value, so it can't double as "not found".</summary>
    Task<decimal?> GetOnHandAsync(int id, CancellationToken ct);

    /// <summary>Weighted-average cost across every PurchaseIn movement for this item (doc 02's
    /// on-hand-costing convention, Phase 3a task brief). Null means the item doesn't exist.</summary>
    Task<decimal?> GetWeightedAverageCostAsync(int id, CancellationToken ct);
}
