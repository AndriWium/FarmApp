using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.Blocks;

public class CreateBlockRequestValidator : AbstractValidator<CreateBlockRequest>
{
    public CreateBlockRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.AreaHectare).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}

public class UpdateBlockRequestValidator : AbstractValidator<UpdateBlockRequest>
{
    public UpdateBlockRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.AreaHectare).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
