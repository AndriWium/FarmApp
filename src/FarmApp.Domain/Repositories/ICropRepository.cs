using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface ICropRepository
{
    Task<Crop?> GetById(int id, CancellationToken ct);
    Task<List<Crop>> GetAll(CancellationToken ct);
    Task Add(Crop crop, CancellationToken ct);
    void Remove(Crop crop);
}
