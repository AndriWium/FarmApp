using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.Customers;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.PriceListId).GreaterThan(0);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0).When(x => x.CreditLimit.HasValue);
    }
}

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.PriceListId).GreaterThan(0);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0).When(x => x.CreditLimit.HasValue);
    }
}
