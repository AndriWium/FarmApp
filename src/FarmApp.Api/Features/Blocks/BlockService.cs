using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Features.Blocks;

public class BlockService(IBlockRepository repo, IUnitOfWork uow) : IBlockService
{
    public Task<List<BlockDto>> GetAllAsync(CancellationToken ct)
        => repo.GetAllAsync(b => new BlockDto(b.BlockId, b.Name, b.AreaHectare, b.Note, b.IsActive), ct);

    public Task<BlockDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, b => new BlockDto(b.BlockId, b.Name, b.AreaHectare, b.Note, b.IsActive), ct);

    public async Task<BlockDto> CreateAsync(CreateBlockRequest request, CancellationToken ct)
    {
        var block = new Block
        {
            Name = request.Name,
            AreaHectare = request.AreaHectare,
            Note = request.Note,
            IsActive = true,
        };
        await repo.AddAsync(block, ct);
        await uow.SaveChangesAsync(ct);
        return new BlockDto(block.BlockId, block.Name, block.AreaHectare, block.Note, block.IsActive);
    }

    public async Task<bool> UpdateAsync(int id, CreateBlockRequest request, CancellationToken ct)
    {
        var block = await repo.GetByIdAsync(id, ct);
        if (block is null) return false;

        block.Name = request.Name;
        block.AreaHectare = request.AreaHectare;
        block.Note = request.Note;
        await uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var block = await repo.GetByIdAsync(id, ct);
        if (block is null) return false;

        repo.Remove(block);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
