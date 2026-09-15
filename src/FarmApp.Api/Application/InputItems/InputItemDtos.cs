using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.InputItems;

public record InputItemDto(
    int InputItemId,
    string Name,
    InputItemCategory Category,
    string Unit,
    decimal ReorderLevel,
    int? WithholdingDays,
    string? ActiveIngredient,
    bool IsActive
    );

public record CreateInputItemRequest(
    string Name,
    InputItemCategory Category,
    string Unit,
    decimal ReorderLevel,
    int? WithholdingDays,
    string? ActiveIngredient
    );

public record UpdateInputItemRequest(
    string Name,
    InputItemCategory Category,
    string Unit,
    decimal ReorderLevel,
    int? WithholdingDays,
    string? ActiveIngredient,
    bool IsActive
    );
