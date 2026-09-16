using FluentValidation;

namespace FarmApp.Api.Application.AccountingPeriods;

/// <summary>doc 10 §1: "Reopening is allowed but loud: Owner role only, reason required,
/// audit-logged." NotEmpty covers both a missing Reason and an all-whitespace one.</summary>
public class ReopenMonthRequestValidator : AbstractValidator<ReopenMonthRequest>
{
    public ReopenMonthRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
