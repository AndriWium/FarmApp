using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.Products;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.ProductType).IsInEnum();
        RuleFor(x => x.CropId).GreaterThan(0).When(x => x.CropId.HasValue);
        RuleFor(x => x.MakeMode).IsInEnum().When(x => x.MakeMode.HasValue);
        RuleFor(x => x.BaseUnit).IsInEnum();
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.ProductType).IsInEnum();
        RuleFor(x => x.CropId).GreaterThan(0).When(x => x.CropId.HasValue);
        RuleFor(x => x.MakeMode).IsInEnum().When(x => x.MakeMode.HasValue);
        RuleFor(x => x.BaseUnit).IsInEnum();
    }
}
