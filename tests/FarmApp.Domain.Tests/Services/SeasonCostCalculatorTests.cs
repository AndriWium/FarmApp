using FarmApp.Domain.Services;

namespace FarmApp.Domain.Tests.Services;

public class SeasonCostCalculatorTests
{
    private readonly SeasonCostCalculator _sut = new();

    // Doc 09's "Worked example, end to end": Season "Tomatoes 2026", expected R3,000 cost /
    // 750 kg -> estimate R4.00/kg.
    [Fact]
    public void CalculateEstimatedCostPerKg_DocExample_ReturnsFourRandPerKg()
    {
        Assert.Equal(4.00m, _sut.CalculateEstimatedCostPerKg(3000m, 750m));
    }

    [Fact]
    public void CalculateEstimatedCostPerKg_NullExpectedTotalCost_ReturnsNull()
    {
        Assert.Null(_sut.CalculateEstimatedCostPerKg(null, 750m));
    }

    [Fact]
    public void CalculateEstimatedCostPerKg_NullExpectedYieldKg_ReturnsNull()
    {
        Assert.Null(_sut.CalculateEstimatedCostPerKg(3000m, null));
    }

    [Fact]
    public void CalculateEstimatedCostPerKg_ZeroExpectedYieldKg_ReturnsNullRatherThanDivideByZero()
    {
        Assert.Null(_sut.CalculateEstimatedCostPerKg(3000m, 0m));
    }

    // Doc 09's headline worked example: Aug R800 + Sep R1500 + Oct R400 + Nov R400 = R3,100 total
    // season cost, 800 kg total harvest -> R3.88/kg actual (3100 / 800 = 3.875, rounded).
    [Fact]
    public void CalculateCostPerKg_DocWorkedExample_ReturnsThreeEightyEightRandPerKg()
    {
        var costs = new SeasonAccumulatedCosts(InputCost: 1700m, LabourCost: 1400m, OverheadAllocated: 0m, TotalKgHarvested: 800m);
        // 1700 + 1400 = 3100 total cost, matching the doc's R3,100 regardless of the input/labour split.
        Assert.Equal(3100m, costs.InputCost + costs.LabourCost + costs.OverheadAllocated);
        Assert.Equal(3.88m, _sut.CalculateCostPerKg(costs));
    }

    [Fact]
    public void CalculateCostPerKg_NoHarvestYet_ReturnsZeroRatherThanDivideByZero()
    {
        // Doc 09's table: "Nov (before harvest) | R3,100 | 0 | ÷0 — undefined" - guarded to 0
        // rather than throwing, since a season can genuinely close having harvested nothing.
        var costs = new SeasonAccumulatedCosts(InputCost: 3100m, LabourCost: 0m, OverheadAllocated: 0m, TotalKgHarvested: 0m);
        Assert.Equal(0m, _sut.CalculateCostPerKg(costs));
    }

    [Fact]
    public void CalculateCostPerKg_IncludesOverheadAllocatedWhenPresent()
    {
        var costs = new SeasonAccumulatedCosts(InputCost: 1000m, LabourCost: 1000m, OverheadAllocated: 100m, TotalKgHarvested: 100m);
        Assert.Equal(21.00m, _sut.CalculateCostPerKg(costs)); // (1000+1000+100)/100 = 21.00
    }

    // Doc 09's true-up narrative: sold kg were charged at the R4.00 estimate but the season
    // actually cost R3.88/kg - COS was overstated, credited back as one true-up adjustment.
    [Fact]
    public void CalculateTrueUpAmount_AllBatchesAtSameEstimate_MatchesDocNarrative()
    {
        var batches = new[] { new HarvestBatchCost(QtyKg: 800m, EstimatedUnitCost: 4.00m) };
        // (3.88 - 4.00) x 800 = -96.00 - a credit, since sales were overcosted during the season.
        Assert.Equal(-96.00m, _sut.CalculateTrueUpAmount(actualCostPerKg: 3.88m, batches));
    }

    [Fact]
    public void CalculateTrueUpAmount_BatchesAtDifferentEstimates_WeightsEachBatchBySnapshottedEstimate()
    {
        // Mid-season estimate change (doc 09: "If mid-season it's clear yield will be poor,
        // update the estimate; new batches use the new rate.") - true-up must honour each
        // batch's own snapshot, not one blended season-wide estimate.
        var batches = new[]
        {
            new HarvestBatchCost(QtyKg: 120m, EstimatedUnitCost: 4.00m),
            new HarvestBatchCost(QtyKg: 680m, EstimatedUnitCost: 3.90m),
        };
        // (3.88-4.00)*120 + (3.88-3.90)*680 = -14.40 + -13.60 = -28.00
        Assert.Equal(-28.00m, _sut.CalculateTrueUpAmount(actualCostPerKg: 3.88m, batches));
    }

    [Fact]
    public void CalculateTrueUpAmount_ActualHigherThanEstimate_ReturnsPositiveUndercostedAmount()
    {
        var batches = new[] { new HarvestBatchCost(QtyKg: 100m, EstimatedUnitCost: 3.00m) };
        Assert.Equal(50.00m, _sut.CalculateTrueUpAmount(actualCostPerKg: 3.50m, batches));
    }

    [Fact]
    public void CalculateTrueUpAmount_NoBatches_ReturnsZero()
    {
        Assert.Equal(0m, _sut.CalculateTrueUpAmount(actualCostPerKg: 3.88m, Array.Empty<HarvestBatchCost>()));
    }
}
