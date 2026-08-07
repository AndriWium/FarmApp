using FluentValidation;

namespace FarmApp.Api.Features.Crops;

public class CreateCropRequestValidator : AbstractValidator<CreateCropRequest>
{
    public CreateCropRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
