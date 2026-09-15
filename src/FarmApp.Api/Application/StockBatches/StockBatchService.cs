using System.Linq.Expressions;
using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Enums;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.StockBatches;

public class StockBatchService(
    IStockBatchRepository repo,
    IStockMovementRepository movementRepo,
    IProductRepository productRepo,
    IGradeRepository gradeRepo,
    IUnitOfWork uow) : IStockBatchService
{
    public Task<List<StockBatchDto>> GetAllAsync(CancellationToken ct)
        => repo.GetAllAsync(ToDto(), ct);

    public Task<StockBatchDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDto(), ct);

    public async Task<decimal?> GetOnHandAsync(int id, CancellationToken ct)
    {
        if (await repo.GetByIdAsync(id, ct) is null) return null;
        return await movementRepo.GetOnHandAsync(id, ct);
    }

    public async Task<ServiceResult<StockBatchDto>> CreateAsync(CreateStockBatchRequest request, CancellationToken ct)
    {
        if (await productRepo.GetByIdAsync(request.ProductId, ct) is null)
            return ServiceResult<StockBatchDto>.Fail(ServiceError.NotFound);

        if (request.GradeId is not null && await gradeRepo.GetByIdAsync(request.GradeId.Value, ct) is null)
            return ServiceResult<StockBatchDto>.Fail(ServiceError.NotFound);

        var batch = new StockBatch
        {
            ProductId = request.ProductId,
            GradeId = request.GradeId,
            Source = request.Source,
            HarvestId = request.HarvestId,
            PurchaseLineId = request.PurchaseLineId,
            Date = request.Date,
            QtyIn = request.QtyIn,
            UnitCost = request.UnitCost,
            ShelfLifeDays = request.ShelfLifeDays,
        };
        await repo.AddAsync(batch, ct);
        // Save now to materialize StockBatchId — StockMovement.StockBatchId is a plain FK column
        // with no navigation property (matches the rest of the codebase's loose-FK convention),
        // so the seeding movement below needs the real generated id before it can be added. This
        // endpoint is a documented stopgap (see DECISIONS.md); the "one SaveChangesAsync per
        // operation" atomicity rule in doc 11 is aimed at the movement-recording service, which
        // never creates new batches and so never hits this two-step problem.
        await uow.SaveChangesAsync(ct);

        // Seed on-hand for the new batch: QtyIn on its own is just a record of what came in —
        // on-hand is always derived from the movement ledger (doc 02), so a batch without a
        // matching "in" movement would show zero on hand despite QtyIn > 0.
        var initialMovementType = request.Source == StockSource.Harvest
            ? StockMovementType.HarvestIn
            : StockMovementType.PurchaseIn;

        await movementRepo.AddRangeAsync([
            new StockMovement
            {
                StockBatchId = batch.StockBatchId,
                Date = request.Date,
                Type = initialMovementType,
                Qty = request.QtyIn,
            }
        ], ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<StockBatchDto>.Ok(new StockBatchDto(
            batch.StockBatchId, batch.ProductId, batch.GradeId, batch.Source,
            batch.HarvestId, batch.PurchaseLineId, batch.ProductionBatchId,
            batch.Date, batch.QtyIn, batch.UnitCost, batch.ShelfLifeDays, batch.BestBeforeDate));
    }

    private static Expression<Func<StockBatch, StockBatchDto>> ToDto()
        => b => new StockBatchDto(
            b.StockBatchId, b.ProductId, b.GradeId, b.Source,
            b.HarvestId, b.PurchaseLineId, b.ProductionBatchId,
            b.Date, b.QtyIn, b.UnitCost, b.ShelfLifeDays, b.BestBeforeDate);
}
