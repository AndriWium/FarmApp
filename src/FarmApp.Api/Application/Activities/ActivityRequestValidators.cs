using FluentValidation;

namespace FarmApp.Api.Application.Activities;

public class CreateActivityInputLineRequestValidator : AbstractValidator<CreateActivityInputLineRequest>
{
    public CreateActivityInputLineRequestValidator()
    {
        RuleFor(x => x.InputItemId).GreaterThan(0);
        RuleFor(x => x.Qty).GreaterThan(0);
    }
}

public class CreateActivityRequestValidator : AbstractValidator<CreateActivityRequest>
{
    public CreateActivityRequestValidator()
    {
        RuleFor(x => x.SeasonId).GreaterThan(0);
        RuleFor(x => x.ActivityTypeId).GreaterThan(0);
        RuleFor(x => x.LabourHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LabourCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(500);
        // Inputs may be empty - not every activity consumes input stock (pruning, weeding...).
        RuleForEach(x => x.Inputs).SetValidator(new CreateActivityInputLineRequestValidator());
    }
}
