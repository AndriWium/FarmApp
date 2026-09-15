using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.StockBatches;
using FarmApp.Api.Application.WithholdingLocks;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Harvests;

public class HarvestService(
    IHarvestRepository repo,
    IHarvestLineRepository lineRepo,
    IStockBatchRepository stockBatchRepo,
    ISeasonRepository seasonRepo,
    IPlantingRepository plantingRepo,
    IProductRepository productRepo,
    IGradeRepository gradeRepo,
    IStockBatchService stockBatchService,
    IWithholdingLockService withholdingLockService,
    IUnitOfWork uow) : IHarvestService
{
    public async Task<List<HarvestDto>> GetAllAsync(int? seasonId, CancellationToken ct)
    {
        var harvests = await repo.GetAllAsync(h => h, seasonId, ct);
        var result = new List<HarvestDto>();
        foreach (var harvest in harvests)
            result.Add(await ToDtoAsync(harvest, ct));
        return result;
    }

    public async Task<HarvestDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var harvest = await repo.GetByIdAsync(id, ct);
        return harvest is null ? null : await ToDtoAsync(harvest, ct);
    }

    public async Task<ServiceResult<HarvestDto>> CreateHarvestAsync(CreateHarvestRequest request, CancellationToken ct)
    {
        // Validate every reference before writing anything - same discipline as
        // ActivityService/ProducePurchaseService.
        var season = await seasonRepo.GetByIdAsync(request.SeasonId, s => new { s.PlantingId }, ct);
        if (season is null)
            return ServiceResult<HarvestDto>.Fail(ServiceError.NotFound);

        // Trace SeasonId -> PlantingId -> BlockId (task brief) to know which block this harvest
        // is happening on, for the withholding-lock check below. Season.PlantingId is itself a
        // validated FK from SeasonService.CreateSeasonAsync (Phase 3a), so a missing Planting here
        // would indicate corrupted data, not a normal client error - still checked defensively
        // rather than assumed.
        var planting = await plantingRepo.GetByIdAsync(season.PlantingId, p => new { p.BlockId }, ct);
        if (planting is null)
            return ServiceResult<HarvestDto>.Fail(ServiceError.NotFound);

        foreach (var line in request.Lines)
        {
            if (await productRepo.GetByIdAsync(line.ProductId, ct) is null)
                return ServiceResult<HarvestDto>.Fail(ServiceError.NotFound);

            if (line.GradeId is not null && await gradeRepo.GetByIdAsync(line.GradeId.Value, ct) is null)
                return ServiceResult<HarvestDto>.Fail(ServiceError.NotFound);
        }

        // The safety-critical check (doc 05 §5): a soft block with a required, traceable
        // override, not a hard rejection with no way through. Resolved before writing anything,
        // same pre-validation discipline as the reference checks above.
        var lockStatus = await withholdingLockService.GetLockStatusAsync(planting.BlockId, request.Date, ct);
        string? overrideReason = null;
        if (lockStatus is { IsLocked: true })
        {
            if (string.IsNullOrWhiteSpace(request.WithholdingOverrideReason))
                return ServiceResult<HarvestDto>.Fail(ServiceError.WithholdingLocked, lockStatus.Reason!);

            // A human explicitly overrode the lock - the reason survives as normal, queryable data
            // on the Harvest itself (task brief), not just whatever the generic audit interceptor
            // separately logs for the insert.
            overrideReason = request.WithholdingOverrideReason;
        }

        // Everything above is deterministic from the request alone; only the per-line StockBatch
        // creation can still legitimately fail from here (defensively - every product/grade was
        // already validated) - so the real transaction starts now. Same genuine-EF-Core-transaction
        // pattern as ActivityService/SaleService, per the task brief's explicit instruction to use
        // it here rather than ProducePurchaseService's older two-phase-save stopgap. Harvest only
        // *adds* stock (no depleting mid-loop availability check), so there's no per-line-flush
        // staleness risk the way ActivityService/SaleService have - but the whole header+lines+
        // batches write still needs to roll back together on any failure.
        await uow.BeginTransactionAsync(ct);
        try
        {
            var harvest = new Harvest
            {
                SeasonId = request.SeasonId,
                Date = request.Date,
                PickedBy = request.PickedBy,
                Notes = request.Notes,
                WithholdingOverrideReason = overrideReason,
            };
            await repo.AddAsync(harvest, ct);
            // Materialize HarvestId before it's used below as every line's HarvestId FK and as
            // StockBatch.HarvestId.
            await uow.SaveChangesAsync(ct);

            var lines = request.Lines.Select(l => new HarvestLine
            {
                HarvestId = harvest.HarvestId,
                ProductId = l.ProductId,
                GradeId = l.GradeId,
                QtyKg = l.QtyKg,
            }).ToList();
            await lineRepo.AddRangeAsync(lines, ct);
            // Materialize each HarvestLineId (used only for the response DTO here - StockBatch has
            // no per-line pointer for Harvest, see DECISIONS.md).
            await uow.SaveChangesAsync(ct);

            // One StockBatch (+ seeding HarvestIn movement) per line, reusing StockBatchService
            // rather than duplicating its batch+movement creation logic (task brief). Every
            // product/grade reference was already validated above, so this is expected to always
            // succeed - defensively roll back the whole harvest if it somehow doesn't, rather than
            // leave a partially-built harvest behind.
            var lineDtos = new List<HarvestLineDto>();
            foreach (var (line, lineRequest) in lines.Zip(request.Lines))
            {
                var batchResult = await stockBatchService.CreateAsync(new CreateStockBatchRequest(
                    line.ProductId, line.GradeId, StockSource.Harvest,
                    HarvestId: harvest.HarvestId, PurchaseLineId: null,
                    harvest.Date, line.QtyKg, lineRequest.UnitCost, lineRequest.ShelfLifeDays), ct);

                if (batchResult.Error != ServiceError.None)
                {
                    await uow.RollbackTransactionAsync(ct);
                    return ServiceResult<HarvestDto>.Fail(
                        batchResult.Error, batchResult.Detail ?? "Failed to create stock batch for harvest line.");
                }

                lineDtos.Add(new HarvestLineDto(
                    line.HarvestLineId, line.ProductId, line.GradeId, line.QtyKg, lineRequest.UnitCost,
                    batchResult.Value!.StockBatchId));
            }

            await uow.CommitTransactionAsync(ct);

            return ServiceResult<HarvestDto>.Ok(new HarvestDto(
                harvest.HarvestId, harvest.SeasonId, harvest.Date, harvest.PickedBy, harvest.Notes,
                harvest.WithholdingOverrideReason, lineDtos));
        }
        catch
        {
            await uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<HarvestDto> ToDtoAsync(Harvest harvest, CancellationToken ct)
    {
        var lines = await lineRepo.GetByHarvestIdAsync(harvest.HarvestId,
            l => new { l.HarvestLineId, l.ProductId, l.GradeId, l.QtyKg }, ct);
        var batches = await stockBatchRepo.GetByHarvestIdAsync(harvest.HarvestId,
            b => new { b.StockBatchId, b.UnitCost }, ct);

        // StockBatch.HarvestId points at the harvest header, not a specific line (doc 02 - unlike
        // ProducePurchaseLine, there's no per-line pointer on StockBatch for Harvest) - lines and
        // batches are created 1:1 in the same order inside CreateHarvestAsync's loop, so ordering
        // both by their identity column and zipping positionally reconstructs the pairing (see
        // DECISIONS.md for the limitation this carries).
        var orderedLines = lines.OrderBy(l => l.HarvestLineId).ToList();
        var orderedBatches = batches.OrderBy(b => b.StockBatchId).ToList();

        var lineDtos = orderedLines
            .Zip(orderedBatches, (l, b) => new HarvestLineDto(l.HarvestLineId, l.ProductId, l.GradeId, l.QtyKg, b.UnitCost, b.StockBatchId))
            .ToList();

        return new HarvestDto(
            harvest.HarvestId, harvest.SeasonId, harvest.Date, harvest.PickedBy, harvest.Notes,
            harvest.WithholdingOverrideReason, lineDtos);
    }
}
