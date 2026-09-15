using FarmApp.Domain.Services;

namespace FarmApp.Domain.Tests.Services;

public class SaleLineCalculatorTests
{
    private readonly SaleLineCalculator _sut = new();

    [Fact]
    public void ToBaseUnitQty_NoPackSize_ReturnsQtyUnchanged()
    {
        // PackSizeId null - Qty is already in the product's base unit (e.g. loose kg).
        Assert.Equal(2.5m, _sut.ToBaseUnitQty(2.5m, null));
    }

    [Fact]
    public void ToBaseUnitQty_WithPackSize_MultipliesByConversionFactor()
    {
        // 3 punnets x 0.25kg/punnet = 0.75kg actually depleted - not 3kg.
        Assert.Equal(0.75m, _sut.ToBaseUnitQty(3m, 0.25m));
    }

    [Fact]
    public void ToBaseUnitQty_PackSizeOfOne_BehavesLikeNoPackSize()
    {
        Assert.Equal(4m, _sut.ToBaseUnitQty(4m, 1m));
    }

    [Fact]
    public void WeightedAverageCost_SingleBatch_ReturnsThatBatchsCost()
    {
        var allocations = new[] { new CostedAllocation(5m, 10m) };
        Assert.Equal(10m, _sut.WeightedAverageCost(allocations));
    }

    [Fact]
    public void WeightedAverageCost_SpansTwoBatchesAtDifferentCosts_ReturnsQtyWeightedAverage()
    {
        // 3kg @ R10/kg + 2kg @ R15/kg = (30+30)/5 = R12/kg - not a plain (10+15)/2 average, and
        // not just the oldest (first) batch's R10 cost either.
        var allocations = new[] { new CostedAllocation(3m, 10m), new CostedAllocation(2m, 15m) };
        Assert.Equal(12m, _sut.WeightedAverageCost(allocations));
    }

    [Fact]
    public void WeightedAverageCost_SpansThreeBatches_WeightsEachByItsOwnQty()
    {
        // 1kg @ R4 + 1kg @ R8 + 2kg @ R10 = (4+8+20)/4 = R8/kg.
        var allocations = new[]
        {
            new CostedAllocation(1m, 4m), new CostedAllocation(1m, 8m), new CostedAllocation(2m, 10m),
        };
        Assert.Equal(8m, _sut.WeightedAverageCost(allocations));
    }

    [Fact]
    public void WeightedAverageCost_EmptyAllocations_Throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.WeightedAverageCost(Array.Empty<CostedAllocation>()));
    }
}
