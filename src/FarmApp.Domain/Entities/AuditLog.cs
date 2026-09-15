namespace FarmApp.Domain.Entities;

/// <summary>Written automatically by AuditInterceptor for every tracked Added/Modified/Deleted
/// entity on save (AI Guide/10-go-live-controls.md, /12-implementation-handoff.md). No business
/// code writes here directly — same "cross-cutting, one place" philosophy as request logging.</summary>
public class AuditLog
{
    public int AuditLogId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? UserName { get; set; }
    public string EntityName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Action { get; set; } = null!;   // "Insert" | "Update" | "Delete"
    public string? Changes { get; set; }           // JSON: changed-property summary
}
