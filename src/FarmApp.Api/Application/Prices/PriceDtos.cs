namespace FarmApp.Api.Application.Prices;

public record PriceDto(
    int PriceId,
    int PriceListId,
    int ProductId,
    int? GradeId,
    int? PackSizeId,
    decimal UnitPrice,
    DateOnly ValidFrom,
    DateOnly? ValidTo
    );

/// <summary>The only write to Price: sets a new price effective from ValidFrom (default
/// today), superseding whatever was previously active for the same combination.</summary>
public record SetPriceRequest(
    int PriceListId,
    int ProductId,
    int? GradeId,
    int? PackSizeId,
    decimal UnitPrice,
    DateOnly? ValidFrom
    );
