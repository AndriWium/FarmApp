namespace FarmApp.Api.Application.InputPurchases;

public record InputPurchaseLineDto(int InputPurchaseLineId, int InputItemId, decimal Qty, decimal UnitCost);

public record InputPurchaseDto(
    int InputPurchaseId, int SupplierId, DateTime Date, string? InvoiceRef, List<InputPurchaseLineDto> Lines);

public record CreateInputPurchaseLineRequest(int InputItemId, decimal Qty, decimal UnitCost);

public record CreateInputPurchaseRequest(int SupplierId, DateTime Date, string? InvoiceRef, List<CreateInputPurchaseLineRequest> Lines);
