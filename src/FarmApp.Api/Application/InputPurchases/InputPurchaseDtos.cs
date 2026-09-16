namespace FarmApp.Api.Application.InputPurchases;

public record InputPurchaseLineDto(int InputPurchaseLineId, int InputItemId, decimal Qty, decimal UnitCost, decimal? VatAmount);

public record InputPurchaseDto(
    int InputPurchaseId, int SupplierId, DateTime Date, string? InvoiceRef, List<InputPurchaseLineDto> Lines);

/// <summary>VatAmount is optional (doc 10 §2's VAT-readiness gap) - typing the VAT shown on the
/// supplier's slip costs 5 seconds and makes history reclaimable/reportable if VAT registration
/// comes later.</summary>
public record CreateInputPurchaseLineRequest(int InputItemId, decimal Qty, decimal UnitCost, decimal? VatAmount = null);

public record CreateInputPurchaseRequest(int SupplierId, DateTime Date, string? InvoiceRef, List<CreateInputPurchaseLineRequest> Lines);
