using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Blocks;

public class BlockService(IBlockRepository repo, IUnitOfWork uow) : IBlockService
{
    public Task<List<BlockDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => repo.GetAllAsync(b => new BlockDto(b.BlockId, b.Name, b.AreaHectare, b.Note, b.IsActive), includeInactive, ct);

    public Task<BlockDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, b => new BlockDto(b.BlockId, b.Name, b.AreaHectare, b.Note, b.IsActive), ct);

    public async Task<ServiceResult<BlockDto>> CreateAsync(CreateBlockRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByNameAsync(request.Name, excludeId: null, ct))
            return ServiceResult<BlockDto>.Fail(ServiceError.DuplicateName);

        var block = new Block
        {
            Name = request.Name,
            AreaHectare = request.AreaHectare,
            Note = request.Note,
        };
        await repo.AddAsync(block, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<BlockDto>.Ok(
            new BlockDto(block.BlockId, block.Name, block.AreaHectare, block.Note, block.IsActive));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateBlockRequest request, CancellationToken ct)
    {
        var block = await repo.GetByIdAsync(id, ct);
        if (block is null) return ServiceError.NotFound;

        if (await repo.ExistsByNameAsync(request.Name, excludeId: id, ct))
            return ServiceError.DuplicateName;

        block.Name = request.Name;
        block.AreaHectare = request.AreaHectare;
        block.Note = request.Note;
        block.IsActive = request.IsActive;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    public async Task<ServiceError> DeactivateAsync(int id, CancellationToken ct)
    {
        var block = await repo.GetByIdAsync(id, ct);
        if (block is null) return ServiceError.NotFound;

        block.IsActive = false;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
