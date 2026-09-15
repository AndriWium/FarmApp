using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.Cultivars;

public class CreateCultivarRequestValidator : AbstractValidator<CreateCultivarRequest>
{
    public CreateCultivarRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.CropId).GreaterThan(0);
    }
}

public class UpdateCultivarRequestValidator : AbstractValidator<UpdateCultivarRequest>
{
    public UpdateCultivarRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.CropId).GreaterThan(0);
    }
}
