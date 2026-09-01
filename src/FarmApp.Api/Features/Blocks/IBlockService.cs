namespace FarmApp.Api.Features.Blocks;

public interface IBlockService
{
    Task<List<BlockDto>> GetAllAsync(CancellationToken ct);
    Task<BlockDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<BlockDto> CreateAsync(CreateBlockRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, CreateBlockRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
