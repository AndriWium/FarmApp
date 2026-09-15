using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.InputItems;

public class InputItemService(IInputItemRepository repo, IUnitOfWork uow) : IInputItemService
{
    public Task<List<InputItemDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(
            x => new InputItemDto(x.InputItemId, x.Name, x.Category, x.Unit, x.ReorderLevel, x.WithholdingDays, x.ActiveIngredient, x.IsActive),
            includeInactive, ct);

    public Task<InputItemDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(
            id,
            x => new InputItemDto(x.InputItemId, x.Name, x.Category, x.Unit, x.ReorderLevel, x.WithholdingDays, x.ActiveIngredient, x.IsActive),
            ct);

    public async Task<ServiceResult<InputItemDto>> CreateAsync(CreateInputItemRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByNameAsync(request.Name, excludeId: null, ct))
            return ServiceResult<InputItemDto>.Fail(ServiceError.DuplicateName);

        var inputItem = new InputItem
        {
            Name = request.Name,
            Category = request.Category,
            Unit = request.Unit,
            ReorderLevel = request.ReorderLevel,
            WithholdingDays = request.WithholdingDays,
            ActiveIngredient = request.ActiveIngredient,
        };
        await repo.AddAsync(inputItem, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<InputItemDto>.Ok(ToDto(inputItem));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateInputItemRequest request, CancellationToken ct)
    {
        var inputItem = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (inputItem is null) return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        inputItem.Name = request.Name;
        inputItem.Category = request.Category;
        inputItem.Unit = request.Unit;
        inputItem.ReorderLevel = request.ReorderLevel;
        inputItem.WithholdingDays = request.WithholdingDays;
        inputItem.ActiveIngredient = request.ActiveIngredient;
        inputItem.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var inputItem = await repo.GetByIdAsync(id, ct);
        if (inputItem is null) return ServiceError.NotFound;

        inputItem.IsActive = false;   // soft delete: master data is never hard-deleted
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    private static InputItemDto ToDto(InputItem x) => new(
        x.InputItemId, x.Name, x.Category, x.Unit, x.ReorderLevel, x.WithholdingDays, x.ActiveIngredient, x.IsActive);
}
