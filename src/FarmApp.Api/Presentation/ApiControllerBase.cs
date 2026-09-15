using FarmApp.Api.Application.Common;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Presentation;

/// <summary>Base for feature controllers — shared translations from validation
/// and service outcomes into consistent HTTP responses. Presentation-layer only:
/// it knows about ActionResult/HTTP status codes, nothing about how a feature works.</summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ValidationProblem(ValidationResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        return ValidationProblem(ModelState);
    }

    /// <summary>Maps a non-None ServiceError to its HTTP response. Call only when Error != None.</summary>
    protected ActionResult ErrorResult(ServiceError error, string entityName) => error switch
    {
        ServiceError.NotFound => NotFound(),
        ServiceError.DuplicateName => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Duplicate name",
            detail: $"A {entityName} with this name already exists (it may be deactivated)."),
        _ => Problem(statusCode: StatusCodes.Status500InternalServerError),
    };
}
