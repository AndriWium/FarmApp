using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Expenses;

public class ExpenseService(
    IExpenseRepository repo,
    IExpenseCategoryRepository categoryRepo,
    ISupplierRepository supplierRepo,
    ISeasonRepository seasonRepo,
    IUnitOfWork uow) : IExpenseService
{
    public Task<ExpenseDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDtoExpr, ct);

    public Task<List<ExpenseDto>> GetAllAsync(DateTime? from, DateTime? to, int? categoryId, CancellationToken ct)
        => repo.GetAllAsync(ToDtoExpr, from, to, categoryId, ct);

    public async Task<ServiceResult<ExpenseDto>> CreateAsync(CreateExpenseRequest request, CancellationToken ct)
    {
        // Validate every reference before writing anything (ProducePurchaseService precedent):
        // ExpenseCategoryId is required, SupplierId/SeasonId are validated only when supplied.
        if (await categoryRepo.GetByIdAsync(request.ExpenseCategoryId, ct) is null)
            return ServiceResult<ExpenseDto>.Fail(ServiceError.NotFound);

        if (request.SupplierId is not null && await supplierRepo.GetByIdAsync(request.SupplierId.Value, ct) is null)
            return ServiceResult<ExpenseDto>.Fail(ServiceError.NotFound);

        if (request.SeasonId is not null && await seasonRepo.GetByIdAsync(request.SeasonId.Value, ct) is null)
            return ServiceResult<ExpenseDto>.Fail(ServiceError.NotFound);

        var expense = new Expense
        {
            Date = request.Date,
            ExpenseCategoryId = request.ExpenseCategoryId,
            Amount = request.Amount,
            VatAmount = request.VatAmount,
            SupplierId = request.SupplierId,
            SeasonId = request.SeasonId,
            Notes = request.Notes,
            AttachmentPath = request.AttachmentPath,
        };
        await repo.AddAsync(expense, ct);
        await uow.SaveChangesAsync(ct); // single-entity write, one atomic save

        return ServiceResult<ExpenseDto>.Ok(ToDto(expense));
    }

    private static readonly System.Linq.Expressions.Expression<Func<Expense, ExpenseDto>> ToDtoExpr = x =>
        new ExpenseDto(x.ExpenseId, x.Date, x.ExpenseCategoryId, x.Amount, x.VatAmount, x.SupplierId, x.SeasonId, x.Notes, x.AttachmentPath);

    private static ExpenseDto ToDto(Expense x) =>
        new(x.ExpenseId, x.Date, x.ExpenseCategoryId, x.Amount, x.VatAmount, x.SupplierId, x.SeasonId, x.Notes, x.AttachmentPath);
}
