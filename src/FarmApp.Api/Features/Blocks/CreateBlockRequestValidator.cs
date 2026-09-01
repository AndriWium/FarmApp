using FarmApp.Api.Shared;
using FluentValidation;

namespace FarmApp.Api.Features.Blocks;

public class CreateBlockRequestValidator : AbstractValidator<CreateBlockRequest>
{
    public CreateBlockRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.AreaHectare).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
