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
}
