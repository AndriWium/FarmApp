using FluentValidation;

namespace FarmApp.Api.Features.Blocks;

public class CreateBlockRequestValidator : AbstractValidator<CreateBlockRequest>
{
    public CreateBlockRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AreaHectare).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
