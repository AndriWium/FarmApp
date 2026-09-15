using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.CustomerPayments;

public record CustomerPaymentDto(
    int CustomerPaymentId, int CustomerId, DateTime Date, decimal Amount, CustomerPaymentMethod Method, string? Ref);

/// <summary>Date is always server-assigned (DateTime.UtcNow), never client-supplied - matches
/// Sale.DateTime/TillSession.OpenedAt's precedent of never trusting a client-supplied
/// transaction timestamp.</summary>
public record CreateCustomerPaymentRequest(int CustomerId, decimal Amount, CustomerPaymentMethod Method, string? Ref);

/// <summary>Balance is always derived (doc 02's "derive, don't store" rule, same as stock
/// on-hand) - never a persisted column anywhere.</summary>
public record CustomerBalanceDto(int CustomerId, decimal Balance);
