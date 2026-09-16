using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.RoadmapItems;

public class CreateRoadmapItemRequestValidator : AbstractValidator<CreateRoadmapItemRequest>
{
    public CreateRoadmapItemRequestValidator()
    {
        RuleFor(x => x.Title).RequiredName(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.TargetPhase).MaximumLength(50);
    }
}

public class UpdateRoadmapItemRequestValidator : AbstractValidator<UpdateRoadmapItemRequest>
{
    public UpdateRoadmapItemRequestValidator()
    {
        RuleFor(x => x.Title).RequiredName(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.TargetPhase).MaximumLength(50);
    }
}
