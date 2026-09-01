using FarmApp.Api.Shared;
using FluentValidation;

namespace FarmApp.Api.Features.Grades;

public class CreateGradeRequestValidator : AbstractValidator<CreateGradeRequest>
{
    public CreateGradeRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
    }
}
