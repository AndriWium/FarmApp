using FluentValidation;

namespace FarmApp.Api.Application.Prices;

public class SetPriceRequestValidator : AbstractValidator<SetPriceRequest>
{
    public SetPriceRequestValidator()
    {
        RuleFor(x => x.PriceListId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.GradeId).GreaterThan(0).When(x => x.GradeId.HasValue);
        RuleFor(x => x.PackSizeId).GreaterThan(0).When(x => x.PackSizeId.HasValue);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}
