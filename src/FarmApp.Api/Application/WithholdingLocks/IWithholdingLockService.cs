namespace FarmApp.Api.Application.WithholdingLocks;

public interface IWithholdingLockService
{
    /// <summary>Null only if blockId doesn't reference a real Block (GetByIdAsync-nullable
    /// convention, matching ICustomerPaymentService.GetBalanceAsync's precedent) - there's no
    /// write here to fail via ServiceResult.</summary>
    Task<WithholdingStatusDto?> GetLockStatusAsync(int blockId, DateTime harvestDate, CancellationToken ct);
}
