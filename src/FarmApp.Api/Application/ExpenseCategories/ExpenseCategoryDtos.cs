namespace FarmApp.Api.Application.ExpenseCategories;

public record ExpenseCategoryDto(int ExpenseCategoryId, string Name, bool IsFarmingDirect, bool IsActive);

public record CreateExpenseCategoryRequest(string Name, bool IsFarmingDirect);

public record UpdateExpenseCategoryRequest(string Name, bool IsFarmingDirect, bool IsActive);
