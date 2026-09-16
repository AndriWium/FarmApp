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
        RuleFor(x => x.ExpectedTotalCost).GreaterThanOrEqualTo(0).When(x => x.ExpectedTotalCost is not null);
        // Strictly positive, not just >= 0 - it's a divisor for EstimatedCostPerKg (doc 09); a
        // zero expected yield can never produce a usable estimate.
        RuleFor(x => x.ExpectedYieldKg).GreaterThan(0).When(x => x.ExpectedYieldKg is not null);
    }
}

public class UpdateSeasonRequestValidator : AbstractValidator<UpdateSeasonRequest>
{
    public UpdateSeasonRequestValidator()
    {
        RuleFor(x => x.PlantingId).GreaterThan(0);
        RuleFor(x => x.Name).RequiredName();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.ExpectedTotalCost).GreaterThanOrEqualTo(0).When(x => x.ExpectedTotalCost is not null);
        RuleFor(x => x.ExpectedYieldKg).GreaterThan(0).When(x => x.ExpectedYieldKg is not null);
    }
}
