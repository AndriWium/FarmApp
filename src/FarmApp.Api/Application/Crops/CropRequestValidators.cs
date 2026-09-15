using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.Crops;

public class CreateCropRequestValidator : AbstractValidator<CreateCropRequest>
{
    public CreateCropRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
    }
}

public class UpdateCropRequestValidator : AbstractValidator<UpdateCropRequest>
{
    public UpdateCropRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
    }
}
