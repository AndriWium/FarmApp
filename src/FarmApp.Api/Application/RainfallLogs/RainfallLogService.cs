using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.RainfallLogs;

public class RainfallLogService(IRainfallLogRepository repo, IUnitOfWork uow) : IRainfallLogService
{
    public Task<List<RainfallLogDto>> GetAllAsync(DateOnly? from, DateOnly? to, CancellationToken ct)
        => repo.GetAllAsync(x => new RainfallLogDto(x.RainfallLogId, x.Date, x.Mm, x.Notes), from, to, ct);

    public Task<RainfallLogDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, x => new RainfallLogDto(x.RainfallLogId, x.Date, x.Mm, x.Notes), ct);

    public async Task<ServiceResult<RainfallLogDto>> CreateAsync(CreateRainfallLogRequest request, CancellationToken ct)
    {
        if (await repo.ExistsByDateAsync(request.Date, excludeId: null, ct))
            return ServiceResult<RainfallLogDto>.Fail(ServiceError.DuplicateName);

        var rainfallLog = new RainfallLog { Date = request.Date, Mm = request.Mm, Notes = request.Notes };
        await repo.AddAsync(rainfallLog, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<RainfallLogDto>.Ok(
            new RainfallLogDto(rainfallLog.RainfallLogId, rainfallLog.Date, rainfallLog.Mm, rainfallLog.Notes));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateRainfallLogRequest request, CancellationToken ct)
    {
        var rainfallLog = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (rainfallLog is null) return ServiceError.NotFound;

        rainfallLog.Mm = request.Mm;
        rainfallLog.Notes = request.Notes;
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }
}
