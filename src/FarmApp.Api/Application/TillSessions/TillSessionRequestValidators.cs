using FluentValidation;

namespace FarmApp.Api.Application.TillSessions;

public class OpenTillSessionRequestValidator : AbstractValidator<OpenTillSessionRequest>
{
    public OpenTillSessionRequestValidator()
    {
        RuleFor(x => x.LocationId).GreaterThan(0);
    }
}

public class CloseTillSessionRequestValidator : AbstractValidator<CloseTillSessionRequest>
{
    public CloseTillSessionRequestValidator()
    {
        RuleFor(x => x.CardMachineBatchTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DifferenceNote).MaximumLength(500);
    }
}
