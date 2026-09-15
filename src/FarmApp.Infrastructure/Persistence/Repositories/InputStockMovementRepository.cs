using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class InputStockMovementRepository(FarmAppDbContext db) : IInputStockMovementRepository
{
    public async Task<decimal> GetOnHandAsync(int inputItemId, CancellationToken ct)
        => await db.InputStockMovements.AsNoTracking()
            .Where(m => m.InputItemId == inputItemId)
            .SumAsync(m => (decimal?)m.Qty, ct) ?? 0m;

    public async Task<decimal> GetWeightedAverageCostAsync(int inputItemId, CancellationToken ct)
    {
        // Only PurchaseIn rows carry a UnitCost (Consumption/Adjustment rows leave it null) -
        // SumAsync over an empty/all-null sequence throws for non-nullable decimal, hence the
        // (decimal?) cast + ?? 0m pattern already established by GetOnHandAsync/StockMovementRepository.
        var purchaseIns = await db.InputStockMovements.AsNoTracking()
            .Where(m => m.InputItemId == inputItemId && m.Type == InputStockMovementType.PurchaseIn)
            .Select(m => new { m.Qty, m.UnitCost })
            .ToListAsync(ct);

        var totalQty = purchaseIns.Sum(m => m.Qty);
        if (totalQty == 0m) return 0m;

        var totalCost = purchaseIns.Sum(m => m.Qty * (m.UnitCost ?? 0m));
        return totalCost / totalQty;
    }

    public async Task AddRangeAsync(IEnumerable<InputStockMovement> movements, CancellationToken ct)
        => await db.InputStockMovements.AddRangeAsync(movements, ct);
}
