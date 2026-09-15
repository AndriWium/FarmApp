namespace FarmApp.Api.Application.Common;

/// <summary>Business-rule outcomes a service can report without throwing.
/// The Presentation layer maps these to HTTP statuses in one place (ApiControllerBase).</summary>
public enum ServiceError
{
    None,
    NotFound,
    DuplicateName,
    InsufficientStock,

    /// <summary>OpenAsync rejected: this location already has an open TillSession - one open
    /// session per location at a time (task brief).</summary>
    TillSessionAlreadyOpen,

    /// <summary>CreateSaleAsync rejected: the referenced TillSession exists but ClosedAt is
    /// already set - a sale can't happen against a closed till.</summary>
    TillSessionClosed,

    /// <summary>CreateSaleAsync rejected: Σ SalePayment.Amount didn't equal the sale's computed
    /// total (Σ Qty x UnitPrice - DiscountAmount) - no partial/short payments this phase.</summary>
    PaymentMismatch,

    /// <summary>CreateSaleAsync rejected: a SalePayment used Method.Account but no CustomerId was
    /// supplied - an anonymous walk-in sale can't be put on account.</summary>
    AccountPaymentRequiresCustomer,

    /// <summary>CloseAsync rejected: this TillSession's ClosedAt is already set - a till can only
    /// be closed (day-closed) once (Phase 2b task brief).</summary>
    TillSessionAlreadyClosed,

    /// <summary>RefundSaleAsync rejected: this Sale's Status is already Refunded - a sale can only
    /// be refunded once (Phase 2b task brief).</summary>
    SaleAlreadyRefunded,

    /// <summary>CreateHarvestAsync rejected: the harvest date falls inside a chemical
    /// withholding-period lock on this season's block, and no WithholdingOverrideReason was
    /// supplied (doc 05 §5, Phase 3b task brief). A soft block with a required, traceable
    /// override, not a hard rejection with no way through - supplying an override reason on a
    /// retried request proceeds instead of failing again.</summary>
    WithholdingLocked,
}

/// <summary>A service result carrying either a value (Error == None) or a business error.
/// Detail is optional context for the error (e.g. InsufficientStock's requested-vs-available
/// numbers) that ApiControllerBase can surface in the ProblemDetails response.</summary>
public record ServiceResult<T>(T? Value, ServiceError Error, string? Detail = null)
{
    public static ServiceResult<T> Ok(T value) => new(value, ServiceError.None);
    public static ServiceResult<T> Fail(ServiceError error) => new(default, error);
    public static ServiceResult<T> Fail(ServiceError error, string detail) => new(default, error, detail);
}
