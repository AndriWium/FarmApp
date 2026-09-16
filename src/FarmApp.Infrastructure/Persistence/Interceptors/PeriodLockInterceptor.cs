using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FarmApp.Infrastructure.Persistence.Interceptors;

/// <summary>Rejects a save that touches an IPeriodLocked entity whose BusinessDate falls in a
/// Closed AccountingPeriod (AI Guide/10-go-live-controls.md §1). Deliberately inert today: no
/// entity implements IPeriodLocked yet (Sale/StockMovement land in Phase 1/2), so this never
/// finds a matching entry — the table + hook exist from Phase 0 so no write path needs
/// retrofitting once those entities show up.</summary>
public class PeriodLockInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context is not FarmAppDbContext db)
            return await base.SavingChangesAsync(eventData, result, ct);

        var lockedEntries = db.ChangeTracker.Entries<IPeriodLocked>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        if (lockedEntries.Count == 0)
            return await base.SavingChangesAsync(eventData, result, ct);

        var yearMonths = lockedEntries
            .Select(e => (e.Entity.BusinessDate.Year, e.Entity.BusinessDate.Month))
            .Distinct()
            .ToList();

        var closedPeriods = await db.AccountingPeriods
            .Where(p => p.Status == AccountingPeriodStatus.Closed)
            .Select(p => new { p.Year, p.Month })
            .ToListAsync(ct);

        foreach (var (year, month) in yearMonths)
        {
            if (closedPeriods.Any(p => p.Year == year && p.Month == month))
                throw new PeriodLockedException(year, month,
                    $"Cannot save: {year:D4}-{month:D2} is a closed accounting period.");
        }

        return await base.SavingChangesAsync(eventData, result, ct);
    }
}
