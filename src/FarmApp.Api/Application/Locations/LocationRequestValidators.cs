using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.Locations;

public class CreateLocationRequestValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
    }
}

public class UpdateLocationRequestValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
    }
}
