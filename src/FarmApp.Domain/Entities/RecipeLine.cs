namespace FarmApp.Domain.Entities;

/// <summary>Ingredient + qty per 1 unit made of a Prepared product. Not an independent
/// aggregate - it lives entirely inside "what does this product contain" and is always
/// managed as a whole set (see IProductService.SetRecipeAsync), never CRUD'd line by line.</summary>
public class RecipeLine
{
    public int RecipeLineId { get; set; }
    public int ProductId { get; set; } // plain FK column, no navigation (matches PackSize->Product)
    public int InputItemId { get; set; } // plain FK column, no navigation
    public decimal Qty { get; set; } // decimal(18,3)
}
