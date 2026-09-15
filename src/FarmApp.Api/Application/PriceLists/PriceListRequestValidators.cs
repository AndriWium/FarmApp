using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.PriceLists;

public class CreatePriceListRequestValidator : AbstractValidator<CreatePriceListRequest>
{
    public CreatePriceListRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
    }
}

public class UpdatePriceListRequestValidator : AbstractValidator<UpdatePriceListRequest>
{
    public UpdatePriceListRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
    }
}
