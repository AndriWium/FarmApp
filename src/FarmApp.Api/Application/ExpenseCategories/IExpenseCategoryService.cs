using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.ExpenseCategories;

public interface IExpenseCategoryService
{
    Task<List<ExpenseCategoryDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<ExpenseCategoryDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ServiceResult<ExpenseCategoryDto>> CreateAsync(CreateExpenseCategoryRequest request, CancellationToken ct);
    Task<ServiceError> UpdateAsync(int id, UpdateExpenseCategoryRequest request, CancellationToken ct);
    Task<ServiceError> DeactivateAsync(int id, CancellationToken ct);
}
