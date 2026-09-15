namespace FarmApp.Domain.Entities;

/// <summary>Simple master data (Spray, Fertilise, Irrigate, Prune, Weed, Harvest-prep...) -
/// Grade-style pattern: unique Name, soft-delete via IsActive (doc 02/Phase 3a task brief).
/// Category is a plain string, not an enum - the task brief calls this "your call"; a string
/// keeps the set open (a farmer adding an activity type shouldn't need a code change to also
/// pick a new category), unlike InputItem.Category which is a genuinely closed, small set.</summary>
public class ActivityType
{
    public int ActivityTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
