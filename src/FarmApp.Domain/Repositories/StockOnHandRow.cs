namespace FarmApp.Domain.Repositories;

/// <summary>On-hand quantity and value for one Product/Grade combination - a lightweight
/// stand-in for doc 04's future Reporting.StockOnHand view (doc 11: reports bypass repositories
/// normally, but for this phase a straightforward aggregation query here is fine).</summary>
public record StockOnHandRow(
    int ProductId, string ProductName, int? GradeId, string? GradeName, decimal QtyOnHand, decimal Value);
