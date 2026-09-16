using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.Expenses;

public interface IExpenseService
{
    Task<ExpenseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<ExpenseDto>> GetAllAsync(DateTime? from, DateTime? to, int? categoryId, CancellationToken ct);
    Task<ServiceResult<ExpenseDto>> CreateAsync(CreateExpenseRequest request, CancellationToken ct);
}
