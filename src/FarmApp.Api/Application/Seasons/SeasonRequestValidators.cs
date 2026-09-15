using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.Seasons;

public class CreateSeasonRequestValidator : AbstractValidator<CreateSeasonRequest>
{
    public CreateSeasonRequestValidator()
    {
        RuleFor(x => x.PlantingId).GreaterThan(0);
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
    }
}

public class UpdateSeasonRequestValidator : AbstractValidator<UpdateSeasonRequest>
{
    public UpdateSeasonRequestValidator()
    {
        RuleFor(x => x.PlantingId).GreaterThan(0);
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
    }
}
