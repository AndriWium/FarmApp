using FarmApp.Domain.Enums;
using FluentValidation;

namespace FarmApp.Api.Application.StockBatches;

public class CreateStockBatchRequestValidator : AbstractValidator<CreateStockBatchRequest>
{
    public CreateStockBatchRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Source).IsInEnum()
            .Must(s => s is StockSource.Harvest or StockSource.Purchase)
            .WithMessage("Only Harvest and Purchase batches can be created directly this phase — Production needs StockMovement to exist first.");
        RuleFor(x => x.QtyIn).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShelfLifeDays).GreaterThan(0);
    }
}
