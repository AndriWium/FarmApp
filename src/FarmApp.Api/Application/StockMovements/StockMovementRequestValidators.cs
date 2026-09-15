using FluentValidation;

namespace FarmApp.Api.Application.StockMovements;

public class RecordStockMovementRequestValidator : AbstractValidator<RecordStockMovementRequest>
{
    public RecordStockMovementRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Qty).GreaterThan(0); // caller always enters a positive amount; the service decides the sign
        RuleFor(x => x.Reason).MaximumLength(200);
    }
}

public class TransferStockRequestValidator : AbstractValidator<TransferStockRequest>
{
    public TransferStockRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Qty).GreaterThan(0);
        RuleFor(x => x.FromLocationId).GreaterThan(0);
        RuleFor(x => x.ToLocationId).GreaterThan(0);
        RuleFor(x => x.ToLocationId).NotEqual(x => x.FromLocationId).WithMessage("From and To locations must differ.");
        RuleFor(x => x.Reason).MaximumLength(200);
    }
}
