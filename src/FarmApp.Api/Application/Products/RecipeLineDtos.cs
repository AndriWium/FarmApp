using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.Products;

public record RecipeLineDto(int RecipeLineId, int InputItemId, decimal Qty);

/// <summary>One line of a SetRecipe call - no RecipeLineId, this is a request to become
/// the new state, not an edit to an existing row.</summary>
public record RecipeLineRequest(int InputItemId, decimal Qty);

public record SetRecipeRequest(List<RecipeLineRequest> Lines);

public record ProductWithRecipeDto(
    int ProductId,
    string Name,
    ProductType ProductType,
    int? CropId,
    MakeMode? MakeMode,
    ProductBaseUnit BaseUnit,
    bool IsActive,
    List<RecipeLineDto> RecipeLines
    );
