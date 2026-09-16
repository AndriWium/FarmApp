using FluentValidation;

namespace FarmApp.Api.Application.InputPurchases;

public class CreateInputPurchaseLineRequestValidator : AbstractValidator<CreateInputPurchaseLineRequest>
{
    public CreateInputPurchaseLineRequestValidator()
    {
        RuleFor(x => x.InputItemId).GreaterThan(0);
        RuleFor(x => x.Qty).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VatAmount).GreaterThanOrEqualTo(0).When(x => x.VatAmount.HasValue);
    }
}

public class CreateInputPurchaseRequestValidator : AbstractValidator<CreateInputPurchaseRequest>
{
    public CreateInputPurchaseRequestValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.InvoiceRef).MaximumLength(50);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new CreateInputPurchaseLineRequestValidator());
    }
}
