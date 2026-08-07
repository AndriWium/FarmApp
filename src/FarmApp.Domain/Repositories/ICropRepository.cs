using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ICropRepository
{
    Task<Crop?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Crop>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Crop crop, CancellationToken ct);
    void Remove(Crop crop);
}
