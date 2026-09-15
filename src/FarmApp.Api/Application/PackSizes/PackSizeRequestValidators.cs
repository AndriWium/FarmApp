using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.PackSizes;

public class CreatePackSizeRequestValidator : AbstractValidator<CreatePackSizeRequest>
{
    public CreatePackSizeRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.QtyInBaseUnit).GreaterThan(0);
    }
}

public class UpdatePackSizeRequestValidator : AbstractValidator<UpdatePackSizeRequest>
{
    public UpdatePackSizeRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.QtyInBaseUnit).GreaterThan(0);
    }
}
