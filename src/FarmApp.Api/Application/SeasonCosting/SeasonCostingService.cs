using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using FarmApp.Domain.Services;

namespace FarmApp.Api.Application.SeasonCosting;

public class SeasonCostingService(
    ISeasonRepository seasonRepo,
    IActivityRepository activityRepo,
    IActivityInputRepository activityInputRepo,
    IHarvestLineRepository harvestLineRepo,
    IStockBatchRepository stockBatchRepo,
    ISeasonCostSummaryRepository summaryRepo,
    ISeasonCostCalculator calculator,
    IUnitOfWork uow) : ISeasonCostingService
{
    public async Task<ServiceResult<SeasonCostPreviewDto>> PreviewCloseAsync(int seasonId, CancellationToken ct)
    {
        var season = await seasonRepo.GetByIdAsync(seasonId, s => new { s.Status, s.EstimatedCostPerKg }, ct);
        if (season is null)
            return ServiceResult<SeasonCostPreviewDto>.Fail(ServiceError.NotFound);

        // A closed season's true-up is already posted (query the SeasonCostSummary instead of
        // previewing it again) - matches CreateHarvestAsync's own SeasonAlreadyClosed guard.
        if (season.Status == SeasonStatus.Closed)
            return ServiceResult<SeasonCostPreviewDto>.Fail(ServiceError.SeasonAlreadyClosed);

        var (inputCost, labourCost, overheadAllocated, totalKg, actualCostPerKg, trueUp) = await ComputeAsync(seasonId, ct);

        return ServiceResult<SeasonCostPreviewDto>.Ok(new SeasonCostPreviewDto(
            seasonId, inputCost, labourCost, overheadAllocated, totalKg,
            season.EstimatedCostPerKg, actualCostPerKg, trueUp));
    }

    public async Task<ServiceResult<SeasonCostSummaryDto>> ConfirmCloseAsync(int seasonId, CancellationToken ct)
    {
        var season = await seasonRepo.GetByIdAsync(seasonId, ct); // tracked - mutating Status below
        if (season is null)
            return ServiceResult<SeasonCostSummaryDto>.Fail(ServiceError.NotFound);

        // Idempotency guard (task brief) - a season can only be closed (true-up posted) once.
        if (season.Status == SeasonStatus.Closed)
            return ServiceResult<SeasonCostSummaryDto>.Fail(ServiceError.SeasonAlreadyClosed);

        var (inputCost, labourCost, overheadAllocated, totalKg, actualCostPerKg, trueUp) = await ComputeAsync(seasonId, ct);

        var summary = new SeasonCostSummary
        {
            SeasonId = seasonId,
            InputCost = inputCost,
            LabourCost = labourCost,
            OverheadAllocated = overheadAllocated,
            TotalKgHarvested = totalKg,
            CostPerKg = actualCostPerKg,
            TrueUpAmount = trueUp,
            ClosedAt = DateTime.UtcNow,
        };
        await summaryRepo.AddAsync(summary, ct);

        // StockBatch.UnitCost/SaleLine.CostAtSale are permanent snapshots and are never rewritten
        // here (doc 09/02) - the true-up lives solely as this one adjustment record, not as a
        // silent revaluation of historical rows.
        season.Status = SeasonStatus.Closed;

        // Single SaveChangesAsync - both writes (the new summary row, the season's Status flip)
        // commit together in EF Core's own implicit transaction, same as SeasonService's own
        // single-save operations; no explicit BeginTransactionAsync needed since nothing here
        // needs an intermediate id materialized mid-operation.
        await uow.SaveChangesAsync(ct);

        return ServiceResult<SeasonCostSummaryDto>.Ok(new SeasonCostSummaryDto(
            summary.SeasonCostSummaryId, summary.SeasonId, summary.InputCost, summary.LabourCost,
            summary.OverheadAllocated, summary.TotalKgHarvested, summary.CostPerKg, summary.TrueUpAmount, summary.ClosedAt));
    }

    /// <summary>Shared by preview and confirm so both ever compute the actual numbers exactly the
    /// same way - a preview a caller just reviewed should never disagree with what confirming it
    /// a moment later actually posts.</summary>
    private async Task<(decimal InputCost, decimal LabourCost, decimal OverheadAllocated, decimal TotalKg, decimal ActualCostPerKg, decimal TrueUp)>
        ComputeAsync(int seasonId, CancellationToken ct)
    {
        var inputCost = await activityInputRepo.GetTotalInputCostForSeasonAsync(seasonId, ct);
        var labourCost = await activityRepo.GetTotalLabourCostForSeasonAsync(seasonId, ct);
        var totalKg = await harvestLineRepo.GetTotalKgHarvestedForSeasonAsync(seasonId, ct);

        // Doc 09/11: "start by not allocating" - overheads are never computed into this yet.
        const decimal overheadAllocated = 0m;

        var actualCostPerKg = calculator.CalculateCostPerKg(
            new SeasonAccumulatedCosts(inputCost, labourCost, overheadAllocated, totalKg));

        var batches = await stockBatchRepo.GetHarvestBatchesBySeasonIdAsync(seasonId, ct);
        var trueUp = calculator.CalculateTrueUpAmount(actualCostPerKg, batches);

        return (inputCost, labourCost, overheadAllocated, totalKg, actualCostPerKg, trueUp);
    }
}
