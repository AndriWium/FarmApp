using System.Linq.Expressions;
using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.TillSessions;

public class TillSessionService(
    ITillSessionRepository repo,
    ILocationRepository locationRepo,
    ISalePaymentRepository salePaymentRepo,
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

        return ServiceResult<TillSessionDto>.Ok(ToDto(session));
    }

    public async Task<ServiceResult<TillSessionDto>> CloseAsync(
        int tillSessionId, decimal cardMachineBatchTotal, string? differenceNote, CancellationToken ct)
    {
        var session = await repo.GetByIdAsync(tillSessionId, ct); // tracked entity - required to mutate + save
        if (session is null) return ServiceResult<TillSessionDto>.Fail(ServiceError.NotFound);
        if (session.ClosedAt is not null) return ServiceResult<TillSessionDto>.Fail(ServiceError.TillSessionAlreadyClosed);

        // System's own card total, computed from SalePayment rows - never trusted from the client
        // and never the card machine's own figure (that's cardMachineBatchTotal, supplied by the
        // cashier reading the terminal). Only Complete sales count - a sale refunded within this
        // same session before close shouldn't contribute its card payment to the day's total.
        var systemCardTotal = await salePaymentRepo.GetCardTotalForTillSessionAsync(tillSessionId, ct);

        session.ClosedAt = DateTime.UtcNow;
        session.SystemCardTotal = systemCardTotal;
        session.CardMachineBatchTotal = cardMachineBatchTotal;
        session.Difference = systemCardTotal - cardMachineBatchTotal; // positive = over, negative = short
        session.DifferenceNote = differenceNote;

        await uow.SaveChangesAsync(ct); // single-entity write, one atomic save

        return ServiceResult<TillSessionDto>.Ok(ToDto(session));
    }

    private static Expression<Func<TillSession, TillSessionDto>> ToDto()
        => t => new TillSessionDto(
            t.TillSessionId, t.LocationId, t.OpenedAt, t.OpenedBy, t.ClosedAt,
            t.SystemCardTotal, t.CardMachineBatchTotal, t.Difference, t.DifferenceNote);

    private static TillSessionDto ToDto(TillSession t) => new(
        t.TillSessionId, t.LocationId, t.OpenedAt, t.OpenedBy, t.ClosedAt,
        t.SystemCardTotal, t.CardMachineBatchTotal, t.Difference, t.DifferenceNote);
}
