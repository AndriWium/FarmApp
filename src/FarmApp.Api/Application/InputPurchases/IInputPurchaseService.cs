using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.InputPurchases;

public interface IInputPurchaseService
{
    Task<List<InputPurchaseDto>> GetAllAsync(CancellationToken ct);
    Task<InputPurchaseDto?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>Validates the supplier and every line's input item first (nothing is written if
    /// any reference is bad), then inserts the header, inserts each line, and writes one
    /// PurchaseIn InputStockMovement per line (RefTable "InputPurchaseLine", RefId that line's
    /// id) - same three-phase-save discipline as IProducePurchaseService.CreatePurchaseAsync.</summary>
    Task<ServiceResult<InputPurchaseDto>> CreatePurchaseAsync(CreateInputPurchaseRequest request, CancellationToken ct);
}
