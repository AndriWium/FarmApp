using FluentValidation;

namespace FarmApp.Api.Application.Sales;

public class CreateSaleLineRequestValidator : AbstractValidator<CreateSaleLineRequest>
{
    public CreateSaleLineRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Qty).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountReason).MaximumLength(200);
    }
}

public class CreateSalePaymentRequestValidator : AbstractValidator<CreateSalePaymentRequest>
{
    public CreateSalePaymentRequestValidator()
    {
        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.ClientGuid).NotEqual(Guid.Empty);
        RuleFor(x => x.TillSessionId).GreaterThan(0);
        RuleFor(x => x.Channel).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(500);

        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new CreateSaleLineRequestValidator());

        RuleFor(x => x.Payments).NotEmpty();
        RuleForEach(x => x.Payments).SetValidator(new CreateSalePaymentRequestValidator());
    }
}
