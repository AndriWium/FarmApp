using System.Linq.Expressions;
using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using FarmApp.Domain.Services;

namespace FarmApp.Api.Application.Seasons;

public class SeasonService(
    ISeasonRepository repo, IPlantingRepository plantingRepo, ISeasonCostCalculator costCalculator, IUnitOfWork uow)
    : ISeasonService
{
    public Task<List<SeasonDto>> GetAllAsync(int? plantingId, CancellationToken ct)
        => repo.GetAllAsync(ToDtoExpr(), plantingId, ct);

    public Task<SeasonDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDtoExpr(), ct);

    public async Task<ServiceResult<SeasonDto>> CreateAsync(CreateSeasonRequest request, CancellationToken ct)
    {
        if (await plantingRepo.GetByIdAsync(request.PlantingId, ct) is null)
            return ServiceResult<SeasonDto>.Fail(ServiceError.NotFound);

        var season = new Season
        {
            PlantingId = request.PlantingId,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ExpectedTotalCost = request.ExpectedTotalCost,
            ExpectedYieldKg = request.ExpectedYieldKg,
            // Server-computed, never trusted from the client (doc 09) - recomputed from whichever
            // of the two raw inputs were actually supplied, so it can never drift from them.
            EstimatedCostPerKg = costCalculator.CalculateEstimatedCostPerKg(request.ExpectedTotalCost, request.ExpectedYieldKg),
        };
        await repo.AddAsync(season, ct);
        await uow.SaveChangesAsync(ct);
        return ServiceResult<SeasonDto>.Ok(ToDto(season));
    }

    public async Task<ServiceError> UpdateAsync(int id, UpdateSeasonRequest request, CancellationToken ct)
    {
        var season = await repo.GetByIdAsync(id, ct);   // tracked entity — required to mutate + save
        if (season is null) return ServiceError.NotFound;

        // Doc 09: the estimate is "editable while the season is open" - once closed, the true-up
        // has already been posted against whatever estimates the batches were snapshotted with,
        // so nothing about the season's own record should move any more.
        if (season.Status == SeasonStatus.Closed) return ServiceError.SeasonAlreadyClosed;

        if (await plantingRepo.GetByIdAsync(request.PlantingId, ct) is null)
            return ServiceError.NotFound;

        season.PlantingId = request.PlantingId;
        season.Name = request.Name;
        season.StartDate = request.StartDate;
        season.EndDate = request.EndDate;
        season.ExpectedTotalCost = request.ExpectedTotalCost;
        season.ExpectedYieldKg = request.ExpectedYieldKg;
        season.EstimatedCostPerKg = costCalculator.CalculateEstimatedCostPerKg(request.ExpectedTotalCost, request.ExpectedYieldKg);
        await uow.SaveChangesAsync(ct);
        return ServiceError.None;
    }

    private static Expression<Func<Season, SeasonDto>> ToDtoExpr()
        => s => new SeasonDto(
            s.SeasonId, s.PlantingId, s.Name, s.StartDate, s.EndDate, s.Status,
            s.ExpectedTotalCost, s.ExpectedYieldKg, s.EstimatedCostPerKg);

    private static SeasonDto ToDto(Season s) => new(
        s.SeasonId, s.PlantingId, s.Name, s.StartDate, s.EndDate, s.Status,
        s.ExpectedTotalCost, s.ExpectedYieldKg, s.EstimatedCostPerKg);
}
