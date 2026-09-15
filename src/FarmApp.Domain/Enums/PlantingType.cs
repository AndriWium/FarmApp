namespace FarmApp.Domain.Enums;

/// <summary>Annuals (vegetables) get a new Planting per season; perennials (fruit trees) are
/// long-lived plantings with yearly production cycles (doc 01/02).</summary>
public enum PlantingType
{
    Annual,
    Perennial,
}
