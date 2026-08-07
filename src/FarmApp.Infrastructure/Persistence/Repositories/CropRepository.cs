using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class CropRepository(FarmAppDbContext db) : ICropRepository
{
    public Task<Crop?> GetByIdAsync(int id, CancellationToken ct)
    {
        return db.Crops.FirstOrDefaultAsync(x => x.CropId == id, ct);
    }

    public Task<List<Crop>> GetAllAsync(CancellationToken ct)
    {
        return db.Crops.AsNoTracking().ToListAsync(ct);
    } 

    public async Task AddAsync(Crop crop, CancellationToken ct)
    { 
        await db.Crops.AddAsync(crop, ct);
    }

    public void Remove(Crop crop)
    {
        db.Crops.Remove(crop);
    }
}
