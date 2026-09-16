using FluentValidation;

namespace FarmApp.Api.Application.ProducePurchases;

public class CreatePurchaseLineRequestValidator : AbstractValidator<CreatePurchaseLineRequest>
{
    public CreatePurchaseLineRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Qty).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShelfLifeDays).GreaterThan(0);
        RuleFor(x => x.VatAmount).GreaterThanOrEqualTo(0).When(x => x.VatAmount.HasValue);
    }
}

public class CreatePurchaseRequestValidator : AbstractValidator<CreatePurchaseRequest>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.InvoiceRef).MaximumLength(50);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new CreatePurchaseLineRequestValidator());
    }
}
