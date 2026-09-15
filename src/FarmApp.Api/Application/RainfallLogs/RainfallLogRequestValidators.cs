using FluentValidation;

namespace FarmApp.Api.Application.RainfallLogs;

public class CreateRainfallLogRequestValidator : AbstractValidator<CreateRainfallLogRequest>
{
    public CreateRainfallLogRequestValidator()
    {
        RuleFor(x => x.Mm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class UpdateRainfallLogRequestValidator : AbstractValidator<UpdateRainfallLogRequest>
{
    public UpdateRainfallLogRequestValidator()
    {
        RuleFor(x => x.Mm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
