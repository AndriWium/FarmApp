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
        ServiceError.InsufficientStock => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Insufficient stock",
            detail: $"Not enough {entityName} on hand to cover the requested quantity."),
        ServiceError.TillSessionAlreadyOpen => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Till session already open",
            detail: "A till session is already open for this location - close it before opening another."),
        ServiceError.TillSessionClosed => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Till session closed",
            detail: "This till session is closed; sales cannot be recorded against it."),
        ServiceError.PaymentMismatch => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Payment mismatch",
            detail: "Payments do not cover the sale total."),
        ServiceError.AccountPaymentRequiresCustomer => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Account payment requires a customer",
            detail: "An Account payment must be linked to a customer - it can't be an anonymous walk-in sale."),
        ServiceError.TillSessionAlreadyClosed => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Till session already closed",
            detail: "This till session was already closed - it can only be day-closed once."),
        ServiceError.SaleAlreadyRefunded => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Sale already refunded",
            detail: "This sale was already refunded - it can only be refunded once."),
        ServiceError.WithholdingLocked => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Withholding period active",
            detail: "This harvest falls inside a chemical withholding-period lock on the block - " +
                     "supply a WithholdingOverrideReason to proceed anyway."),
        ServiceError.SeasonAlreadyClosed => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Season already closed",
            detail: $"This {entityName} is already closed - it can only be closed once."),
        ServiceError.SeasonEstimateNotSet => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Season estimate not set",
            detail: $"This {entityName} has no cost estimate yet - set ExpectedTotalCost and " +
                     "ExpectedYieldKg on the season before recording a harvest."),
        ServiceError.AccountingPeriodTillSessionsOpen => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Till sessions still open",
            detail: "This period cannot be closed while a till session opened within it is still open - " +
                     "close it first (GET the checklist for exactly which one(s))."),
        ServiceError.AccountingPeriodAlreadyClosed => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Period already closed",
            detail: "This accounting period is already closed - it can only be closed once."),
        ServiceError.AccountingPeriodNotClosed => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Period not closed",
            detail: "This accounting period is not closed - there is nothing to reopen."),
        _ => Problem(statusCode: StatusCodes.Status500InternalServerError),
    };

    /// <summary>Same mapping as ErrorResult, with a precise message for InsufficientStock (the
    /// FIFO service reports exactly how much was requested vs. available) and PaymentMismatch
    /// (the exact amounts involved).</summary>
    protected ActionResult ErrorResult(ServiceError error, string entityName, string detail) => error switch
    {
        ServiceError.InsufficientStock => Problem(statusCode: StatusCodes.Status409Conflict, title: "Insufficient stock", detail: detail),
        ServiceError.PaymentMismatch => Problem(statusCode: StatusCodes.Status409Conflict, title: "Payment mismatch", detail: detail),
        ServiceError.WithholdingLocked => Problem(statusCode: StatusCodes.Status409Conflict, title: "Withholding period active", detail: detail),
        ServiceError.SeasonAlreadyClosed => Problem(statusCode: StatusCodes.Status409Conflict, title: "Season already closed", detail: detail),
        ServiceError.SeasonEstimateNotSet => Problem(statusCode: StatusCodes.Status409Conflict, title: "Season estimate not set", detail: detail),
        ServiceError.AccountingPeriodTillSessionsOpen => Problem(statusCode: StatusCodes.Status409Conflict, title: "Till sessions still open", detail: detail),
        _ => ErrorResult(error, entityName),
    };
}
