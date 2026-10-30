using BusinessLogicLayer.Services;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class OrderTotalCalculatorTests
{
    [Theory]
    [InlineData(100, 1, 20d, 80)]
    [InlineData(59.99, 2, 35d, 77.987)]
    [InlineData(19.99, 3, 0d, 59.97)]
    [InlineData(19.99, 3, null, 59.97)]
    [InlineData(10, 1, 100d, 0)]
    public void LineTotal_ShouldTreatDiscountAsWholePercent(double unitPrice, int quantity, double? discount, double expected)
    {
        // Act
        var result = OrderTotalCalculator.LineTotal((decimal)unitPrice, quantity, discount);

        // Assert
        Assert.Equal((decimal)expected, result);
    }

    [Theory]
    [InlineData(150d, 0)]
    [InlineData(-30d, 10)]
    public void LineTotal_ShouldKeepResultBetweenZeroAndFullPrice(double discount, double expected)
    {
        // Act
        var result = OrderTotalCalculator.LineTotal(10m, 1, discount);

        // Assert
        Assert.Equal((decimal)expected, result);
    }

    [Fact]
    public void Total_ShouldSumAllLines()
    {
        // Arrange
        (decimal UnitPrice, int Quantity, double? DiscountPercent)[] lines =
        [
            (100m, 2, 50d),
            (10m, 3, null),
        ];

        // Act
        var result = OrderTotalCalculator.Total(lines);

        // Assert
        Assert.Equal(130m, result);
    }

    [Fact]
    public void Total_WhenOrderIsEmpty_ShouldBeZero()
    {
        // Act
        var result = OrderTotalCalculator.Total([]);

        // Assert
        Assert.Equal(0m, result);
    }
}
