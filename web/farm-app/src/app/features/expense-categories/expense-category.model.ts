// Mirrors FarmApp.Api.Application.ExpenseCategories.ExpenseCategoryDtos.
export interface ExpenseCategoryDto {
  expenseCategoryId: number;
  name: string;
  isFarmingDirect: boolean;
  isActive: boolean;
}

export interface CreateExpenseCategoryRequest {
  name: string;
  isFarmingDirect: boolean;
}

export interface UpdateExpenseCategoryRequest {
  name: string;
  isFarmingDirect: boolean;
  isActive: boolean;
}
