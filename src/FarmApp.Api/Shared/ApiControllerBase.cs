using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Shared;

/// <summary>Base for feature controllers — turns a FluentValidation result into the same
/// ProblemDetails 400 response every controller used to build by hand.</summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ValidationProblem(ValidationResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        return ValidationProblem(ModelState);
    }
}
