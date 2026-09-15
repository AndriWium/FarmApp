using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.Customers;

public record CustomerDto(
    int CustomerId,
    string Name,
    string? Phone,
    CustomerType Type,
    int PriceListId,
    decimal? CreditLimit,
    bool IsActive
    );

public record CreateCustomerRequest(
    string Name,
    string? Phone,
    CustomerType Type,
    int PriceListId,
    decimal? CreditLimit
    );

public record UpdateCustomerRequest(
    string Name,
    string? Phone,
    CustomerType Type,
    int PriceListId,
    decimal? CreditLimit,
    bool IsActive
    );
