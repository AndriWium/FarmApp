using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.Prices;

public class PriceService(
    IPriceRepository repo,
    IPriceListRepository priceListRepo,
    IProductRepository productRepo,
    IGradeRepository gradeRepo,
    IPackSizeRepository packSizeRepo,
    IUnitOfWork uow) : IPriceService
{
    public async Task<ServiceResult<PriceDto>> SetPriceAsync(SetPriceRequest request, CancellationToken ct)
    {
        if (await priceListRepo.GetByIdAsync(request.PriceListId, ct) is null)
            return ServiceResult<PriceDto>.Fail(ServiceError.NotFound);

        if (await productRepo.GetByIdAsync(request.ProductId, ct) is null)
            return ServiceResult<PriceDto>.Fail(ServiceError.NotFound);

        if (request.GradeId is not null && await gradeRepo.GetByIdAsync(request.GradeId.Value, ct) is null)
            return ServiceResult<PriceDto>.Fail(ServiceError.NotFound);

        if (request.PackSizeId is not null && await packSizeRepo.GetByIdAsync(request.PackSizeId.Value, ct) is null)
            return ServiceResult<PriceDto>.Fail(ServiceError.NotFound);

        var validFrom = request.ValidFrom ?? DateOnly.FromDateTime(DateTime.UtcNow);

        // Close out whatever is currently active for this exact combination (SQL-null-safe
        // equality on GradeId/PackSizeId - see PriceRepository). Exclusive end: the old price
        // is valid up to but not including the new price's start date.
        var active = await repo.GetActiveAsync(request.PriceListId, request.ProductId, request.GradeId, request.PackSizeId, ct);
        if (active is not null)
            active.ValidTo = validFrom;

        var price = new Price
        {
            PriceListId = request.PriceListId,
            ProductId = request.ProductId,
            GradeId = request.GradeId,
            PackSizeId = request.PackSizeId,
            UnitPrice = request.UnitPrice,
            ValidFrom = validFrom,
            ValidTo = null,
        };
        await repo.AddAsync(price, ct);
        await uow.SaveChangesAsync(ct);   // one SaveChanges for the whole supersession

        return ServiceResult<PriceDto>.Ok(ToDto(price));
    }

    public Task<PriceDto?> GetCurrentAsync(int priceListId, int productId, int? gradeId, int? packSizeId, CancellationToken ct)
        => repo.GetCurrentAsync(priceListId, productId, gradeId, packSizeId, ToDtoExpr, ct);

    public Task<List<PriceDto>> GetHistoryAsync(int priceListId, int productId, int? gradeId, int? packSizeId, CancellationToken ct)
        => repo.GetHistoryAsync(priceListId, productId, gradeId, packSizeId, ToDtoExpr, ct);

    private static readonly System.Linq.Expressions.Expression<Func<Price, PriceDto>> ToDtoExpr =
        x => new PriceDto(x.PriceId, x.PriceListId, x.ProductId, x.GradeId, x.PackSizeId, x.UnitPrice, x.ValidFrom, x.ValidTo);

    private static PriceDto ToDto(Price x) =>
        new(x.PriceId, x.PriceListId, x.ProductId, x.GradeId, x.PackSizeId, x.UnitPrice, x.ValidFrom, x.ValidTo);
}
