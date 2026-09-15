using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.InputItems;

public class CreateInputItemRequestValidator : AbstractValidator<CreateInputItemRequest>
{
    public CreateInputItemRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WithholdingDays).GreaterThanOrEqualTo(0).When(x => x.WithholdingDays.HasValue);
        RuleFor(x => x.ActiveIngredient).MaximumLength(100);
    }
}

public class UpdateInputItemRequestValidator : AbstractValidator<UpdateInputItemRequest>
{
    public UpdateInputItemRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WithholdingDays).GreaterThanOrEqualTo(0).When(x => x.WithholdingDays.HasValue);
        RuleFor(x => x.ActiveIngredient).MaximumLength(100);
    }
}
