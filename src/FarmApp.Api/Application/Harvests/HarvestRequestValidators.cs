using FluentValidation;

namespace FarmApp.Api.Application.Harvests;

public class CreateHarvestLineRequestValidator : AbstractValidator<CreateHarvestLineRequest>
{
    public CreateHarvestLineRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.QtyKg).GreaterThan(0);
        RuleFor(x => x.ShelfLifeDays).GreaterThan(0);
    }
}

public class CreateHarvestRequestValidator : AbstractValidator<CreateHarvestRequest>
{
    public CreateHarvestRequestValidator()
    {
        RuleFor(x => x.SeasonId).GreaterThan(0);
        RuleFor(x => x.PickedBy).GreaterThan(0).When(x => x.PickedBy is not null);
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.WithholdingOverrideReason).MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new CreateHarvestLineRequestValidator());
    }
}
