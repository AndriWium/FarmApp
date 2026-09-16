using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.ExpenseCategories;

public class ExpenseCategoryService(IExpenseCategoryRepository repo, IUnitOfWork uow) : IExpenseCategoryService
{
    public Task<List<ExpenseCategoryDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(c => new ExpenseCategoryDto(c.ExpenseCategoryId, c.Name, c.IsFarmingDirect, c.IsActive), includeInactive, ct);

    public Task<ExpenseCategoryDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, c => new ExpenseCategoryDto(c.ExpenseCategoryId, c.Name, c.IsFarmingDirect, c.IsActive), ct);

    public async Task<ServiceResult<ExpenseCategoryDto>> CreateAsync(CreateExpenseCategoryRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByNameAsync(request.Name, excludeId: null, ct))
            return ServiceResult<ExpenseCategoryDto>.Fail(ServiceError.DuplicateName);

        var category = new ExpenseCategory { Name = request.Name, IsFarmingDirect = request.IsFarmingDirect };
        await repo.AddAsync(category, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<ExpenseCategoryDto>.Ok(
            new ExpenseCategoryDto(category.ExpenseCategoryId, category.Name, category.IsFarmingDirect, category.IsActive));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateExpenseCategoryRequest request, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (category is null) return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        category.Name = request.Name;
        category.IsFarmingDirect = request.IsFarmingDirect;
        category.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(id, ct);
        if (category is null) return ServiceError.NotFound;

        category.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
