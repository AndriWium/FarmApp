using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.ActivityTypes;

public class CreateActivityTypeRequestValidator : AbstractValidator<CreateActivityTypeRequest>
{
    public CreateActivityTypeRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
    }
}

public class UpdateActivityTypeRequestValidator : AbstractValidator<UpdateActivityTypeRequest>
{
    public UpdateActivityTypeRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
    }
}
