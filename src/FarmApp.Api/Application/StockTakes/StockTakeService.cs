using FarmApp.Api.Application.Common;
using FarmApp.Api.Application.StockMovements;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.StockTakes;

public class StockTakeService(
    IStockTakeRepository repo,
    IStockTakeLineRepository lineRepo,
    IStockBatchRepository stockBatchRepo,
    IStockMovementRepository movementRepo,
    ILocationRepository locationRepo,
    IStockMovementService stockMovementService,
    IUnitOfWork uow) : IStockTakeService
{
    public async Task<List<StockTakeDto>> GetAllAsync(CancellationToken ct)
    {
        var stockTakes = await repo.GetAllAsync(x => x, ct);
        var result = new List<StockTakeDto>();
        foreach (var stockTake in stockTakes)
            result.Add(await ToDtoAsync(stockTake, ct));
        return result;
    }

    public async Task<StockTakeDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var stockTake = await repo.GetByIdAsync(id, ct);
        return stockTake is null ? null : await ToDtoAsync(stockTake, ct);
    }

    public async Task<ServiceResult<StockTakeDto>> StartStockTakeAsync(StartStockTakeRequest request, CancellationToken ct)
    {
        if (request.LocationId is not null && await locationRepo.GetByIdAsync(request.LocationId.Value, ct) is null)
            return ServiceResult<StockTakeDto>.Fail(ServiceError.NotFound);

        // Validate every batch id exists before writing anything - a stock take never creates
        // batches, it only counts ones that already exist (task brief).
        var batchIds = request.StockBatchIds.Distinct().ToList();
        foreach (var batchId in batchIds)
        {
            if (await stockBatchRepo.GetByIdAsync(batchId, ct) is null)
                return ServiceResult<StockTakeDto>.Fail(ServiceError.NotFound);
        }

        var stockTake = new StockTake
        {
            Date = request.Date,
            LocationId = request.LocationId,
            Notes = request.Notes,
        };
        await repo.AddAsync(stockTake, ct);
        // Materialize StockTakeId before the lines below can reference it - same loose-FK
        // two-phase-save stopgap StockBatchService already established (see DECISIONS.md).
        await uow.SaveChangesAsync(ct);

        var lines = new List<StockTakeLine>();
        foreach (var batchId in batchIds)
        {
            var systemQty = await movementRepo.GetOnHandAsync(batchId, ct);
            lines.Add(new StockTakeLine
            {
                StockTakeId = stockTake.StockTakeId,
                StockBatchId = batchId,
                SystemQty = systemQty,
                CountedQty = null,
                Variance = 0,
            });
        }
        await lineRepo.AddRangeAsync(lines, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<StockTakeDto>.Ok(ToDto(stockTake, lines));
    }

    public async Task<ServiceResult<StockTakeDto>> RecordCountsAsync(int stockTakeId, RecordCountsRequest request, CancellationToken ct)
    {
        var stockTake = await repo.GetByIdAsync(stockTakeId, ct);
        if (stockTake is null) return ServiceResult<StockTakeDto>.Fail(ServiceError.NotFound);

        var lines = await lineRepo.GetByStockTakeIdAsync(stockTakeId, ct); // tracked
        var lineById = lines.ToDictionary(l => l.StockTakeLineId);

        // Validate every StockTakeLineId belongs to this stock take before mutating anything.
        foreach (var count in request.Counts)
        {
            if (!lineById.ContainsKey(count.StockTakeLineId))
                return ServiceResult<StockTakeDto>.Fail(ServiceError.NotFound);
        }

        foreach (var count in request.Counts)
        {
            var line = lineById[count.StockTakeLineId];
            line.CountedQty = count.CountedQty;
            line.Variance = count.CountedQty - line.SystemQty;

            if (line.Variance != 0)
            {
                // Signed both ways - positive (found more) and negative (found less) both go
                // through the same batch, no FIFO allocation needed since the batch is already
                // known (task brief). Doesn't save itself; this whole reconciliation - every
                // line's CountedQty/Variance and every resulting movement - commits in the one
                // SaveChangesAsync below.
                await stockMovementService.RecordBatchAdjustmentAsync(
                    line.StockBatchId, line.Variance, "Stock take reconciliation", stockTake.LocationId, ct);
            }
        }

        await uow.SaveChangesAsync(ct); // one SaveChangesAsync for the whole operation - atomic

        return ServiceResult<StockTakeDto>.Ok(ToDto(stockTake, lines));
    }

    private async Task<StockTakeDto> ToDtoAsync(StockTake stockTake, CancellationToken ct)
    {
        var lines = await lineRepo.GetByStockTakeIdAsync(stockTake.StockTakeId,
            l => new StockTakeLineDto(l.StockTakeLineId, l.StockBatchId, l.CountedQty, l.SystemQty, l.Variance), ct);
        return new StockTakeDto(stockTake.StockTakeId, stockTake.Date, stockTake.LocationId, stockTake.Notes, lines);
    }

    private static StockTakeDto ToDto(StockTake stockTake, List<StockTakeLine> lines)
        => new(stockTake.StockTakeId, stockTake.Date, stockTake.LocationId, stockTake.Notes,
            lines.Select(l => new StockTakeLineDto(l.StockTakeLineId, l.StockBatchId, l.CountedQty, l.SystemQty, l.Variance)).ToList());
}
