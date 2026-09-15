using FluentValidation;

namespace FarmApp.Api.Application.CustomerPayments;

public class CreateCustomerPaymentRequestValidator : AbstractValidator<CreateCustomerPaymentRequest>
{
    public CreateCustomerPaymentRequestValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.Ref).MaximumLength(50);
    }
}
