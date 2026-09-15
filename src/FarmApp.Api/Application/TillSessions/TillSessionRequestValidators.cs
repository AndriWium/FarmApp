using FluentValidation;

namespace FarmApp.Api.Application.TillSessions;

public class OpenTillSessionRequestValidator : AbstractValidator<OpenTillSessionRequest>
{
    public OpenTillSessionRequestValidator()
    {
        RuleFor(x => x.LocationId).GreaterThan(0);
    }
}
