using FluentValidation;

namespace FarmApp.Api.Application.Products;

public class SetRecipeRequestValidator : AbstractValidator<SetRecipeRequest>
{
    public SetRecipeRequestValidator()
    {
        RuleFor(x => x.Lines).NotNull();
        RuleForEach(x => x.Lines).SetValidator(new RecipeLineRequestValidator());
    }
}

public class RecipeLineRequestValidator : AbstractValidator<RecipeLineRequest>
{
    public RecipeLineRequestValidator()
    {
        RuleFor(x => x.InputItemId).GreaterThan(0);
        RuleFor(x => x.Qty).GreaterThan(0);
    }
}
