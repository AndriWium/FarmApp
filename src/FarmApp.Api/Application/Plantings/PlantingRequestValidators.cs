using FluentValidation;

namespace FarmApp.Api.Application.Plantings;

public class CreatePlantingRequestValidator : AbstractValidator<CreatePlantingRequest>
{
    public CreatePlantingRequestValidator()
    {
        RuleFor(x => x.BlockId).GreaterThan(0);
        RuleFor(x => x.CultivarId).GreaterThan(0);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate is not null);
        RuleFor(x => x.PlantCount).GreaterThan(0).When(x => x.PlantCount is not null);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class UpdatePlantingRequestValidator : AbstractValidator<UpdatePlantingRequest>
{
    public UpdatePlantingRequestValidator()
    {
        RuleFor(x => x.BlockId).GreaterThan(0);
        RuleFor(x => x.CultivarId).GreaterThan(0);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate is not null);
        RuleFor(x => x.PlantCount).GreaterThan(0).When(x => x.PlantCount is not null);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
