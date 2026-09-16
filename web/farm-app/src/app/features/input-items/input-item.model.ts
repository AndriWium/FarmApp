// Mirrors FarmApp.Api.Application.InputItems.InputItemDtos + FarmApp.Domain.Enums.InputItemCategory.
// Program.cs configures enums to serialize as strings (JsonStringEnumConverter) - see the comment
// above builder.Services.AddControllers() - so these values travel over the wire as the strings
// below, not as numbers.
export type InputItemCategory = 'Seed' | 'Fertiliser' | 'Chemical' | 'Packaging' | 'Ingredient' | 'Other';

export const INPUT_ITEM_CATEGORIES: InputItemCategory[] = [
  'Seed',
  'Fertiliser',
  'Chemical',
  'Packaging',
  'Ingredient',
  'Other',
];

export interface InputItemDto {
  inputItemId: number;
  name: string;
  category: InputItemCategory;
  unit: string;
  reorderLevel: number;
  withholdingDays: number | null;
  activeIngredient: string | null;
  isActive: boolean;
}

export interface CreateInputItemRequest {
  name: string;
  category: InputItemCategory;
  unit: string;
  reorderLevel: number;
  withholdingDays: number | null;
  activeIngredient: string | null;
}

export interface UpdateInputItemRequest {
  name: string;
  category: InputItemCategory;
  unit: string;
  reorderLevel: number;
  withholdingDays: number | null;
  activeIngredient: string | null;
  isActive: boolean;
}
