using System.Text.Json;
using FarmApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FarmApp.Infrastructure.Persistence.Interceptors;

/// <summary>Writes an AuditLog row for every tracked Added/Modified/Deleted entity on save,
/// capturing the current user's identity via IHttpContextAccessor (AI Guide/10-go-live-controls.md,
/// /12-implementation-handoff.md). Applies immediately — unlike PeriodLockInterceptor there is no
/// marker-interface gate, so Grade/Block/Crop changes start showing up in AuditLog right away.</summary>
public class AuditInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    // Added entities with a store-generated key (identity column) don't have their real id yet
    // at SavingChangesAsync time — entry.Property(...).CurrentValue is EF's temporary placeholder
    // until the main save assigns the database-generated value. Queue those pairs here and patch
    // the real id into the (already in-memory) AuditLog row in SavedChangesAsync, then persist the
    // audit rows in a second, explicit SaveChanges — the documented pattern for EF audit-trail
    // interceptors (see Microsoft's own interceptor sample for the same fix-up).
    private readonly List<(AuditLog Audit, EntityEntry Entry)> _pendingInserts = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context is FarmAppDbContext db)
            AddAuditEntries(db);

        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        if (_pendingInserts.Count > 0 && eventData.Context is FarmAppDbContext db)
        {
            foreach (var (audit, entry) in _pendingInserts)
                audit.EntityId = GetPrimaryKeyValue(entry);

            _pendingInserts.Clear();
            await db.SaveChangesAsync(ct);
        }

        return await base.SavedChangesAsync(eventData, result, ct);
    }

    private void AddAuditEntries(FarmAppDbContext db)
    {
        var userName = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";

        // Materialize first: we're about to add AuditLog entries to the same change tracker,
        // and must not enumerate it while doing so.
        var entries = db.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog
                && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "Insert",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown",
            };

            var audit = new AuditLog
            {
                OccurredAt = DateTime.UtcNow,
                UserName = userName,
                EntityName = entry.Entity.GetType().Name,
                EntityId = GetPrimaryKeyValue(entry),
                Action = action,
                Changes = BuildChanges(entry),
            };
            db.AuditLogs.Add(audit);

            if (entry.State == EntityState.Added && !entry.IsKeySet)
                _pendingInserts.Add((audit, entry));
        }
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return "";
        return string.Join(",", key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? ""));
    }

    private static string BuildChanges(EntityEntry entry)
    {
        var changed = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    changed[prop.Metadata.Name] = prop.CurrentValue;
                    break;
                case EntityState.Deleted:
                    changed[prop.Metadata.Name] = prop.OriginalValue;
                    break;
                case EntityState.Modified when prop.IsModified:
                    changed[prop.Metadata.Name] = new { Old = prop.OriginalValue, New = prop.CurrentValue };
                    break;
            }
        }
        return JsonSerializer.Serialize(changed);
    }
}
