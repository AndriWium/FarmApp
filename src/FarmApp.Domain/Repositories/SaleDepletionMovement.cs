namespace FarmApp.Domain.Repositories;

/// <summary>One StockMovement a Sale's checkout originally created (Type == SaleOut, RefTable
/// "Sale"/RefId the sale's id) - exactly what RefundSaleAsync needs to reverse precisely: which
/// batch, how much (still signed negative, matching the stored row), and which location.</summary>
public record SaleDepletionMovement(int StockBatchId, decimal Qty, int? LocationId);
