using System.Linq.Expressions;
using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.TillSessions;

public class TillSessionService(
    ITillSessionRepository repo,
    ILocationRepository locationRepo,
    IUnitOfWork uow) : ITillSessionService
{
    public Task<TillSessionDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDto(), ct);

    public Task<List<TillSessionDto>> GetAllAsync(int? locationId, bool openOnly, CancellationToken ct)
        => repo.GetAllAsync(ToDto(), locationId, openOnly, ct);

    public async Task<ServiceResult<TillSessionDto>> OpenAsync(int locationId, int openedByUserId, CancellationToken ct)
    {
        if (await locationRepo.GetByIdAsync(locationId, ct) is null)
            return ServiceResult<TillSessionDto>.Fail(ServiceError.NotFound);

        // One open session per location at a time (task brief) - checked before writing anything.
        if (await repo.HasOpenSessionAsync(locationId, ct))
            return ServiceResult<TillSessionDto>.Fail(ServiceError.TillSessionAlreadyOpen);

        var session = new TillSession
        {
            LocationId = locationId,
            OpenedAt = DateTime.UtcNow,
            OpenedBy = openedByUserId,
        };
        await repo.AddAsync(session, ct);
        await uow.SaveChangesAsync(ct); // single-entity write, one atomic save

        return ServiceResult<TillSessionDto>.Ok(new TillSessionDto(
            session.TillSessionId, session.LocationId, session.OpenedAt, session.OpenedBy, session.ClosedAt));
    }

    private static Expression<Func<TillSession, TillSessionDto>> ToDto()
        => t => new TillSessionDto(t.TillSessionId, t.LocationId, t.OpenedAt, t.OpenedBy, t.ClosedAt);
}
