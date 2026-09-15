using FluentValidation;

namespace FarmApp.Api.Application.StockTakes;

public class StartStockTakeRequestValidator : AbstractValidator<StartStockTakeRequest>
{
    public StartStockTakeRequestValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.StockBatchIds).NotEmpty();
        RuleForEach(x => x.StockBatchIds).GreaterThan(0);
    }
}

public class CountLineRequestValidator : AbstractValidator<CountLineRequest>
{
    public CountLineRequestValidator()
    {
        RuleFor(x => x.StockTakeLineId).GreaterThan(0);
        RuleFor(x => x.CountedQty).GreaterThanOrEqualTo(0);
    }
}

public class RecordCountsRequestValidator : AbstractValidator<RecordCountsRequest>
{
    public RecordCountsRequestValidator()
    {
        RuleFor(x => x.Counts).NotEmpty();
        RuleForEach(x => x.Counts).SetValidator(new CountLineRequestValidator());
    }
}
