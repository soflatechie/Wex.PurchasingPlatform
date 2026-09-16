using Wex.PurchasingPlatform.Api.Common;

namespace Wex.PurchasingPlatform.Tests.Common;

public class MoneyRoundingTests
{
    [Fact]
    public void ToCurrency_WhenValueHasThreeDecimalPlaces_RoundsAwayFromZeroToTheNearestCent()
    {
        var result = MoneyRounding.ToCurrency(42.555m);

        Assert.Equal(42.56m, result);
    }

    [Theory]
    [InlineData(RoundingStrategy.AwayFromZero, 42.56)]
    [InlineData(RoundingStrategy.ToEven, 42.56)]
    [InlineData(RoundingStrategy.Up, 42.56)]
    [InlineData(RoundingStrategy.Down, 42.55)]
    public void ToCurrency_WithEachStrategy_RoundsAsExpected(RoundingStrategy strategy, decimal expected)
    {
        var result = MoneyRounding.ToCurrency(42.555m, strategy);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToCurrency_ToEven_RoundsHalfwayValueToTheNearestEvenCent()
    {
        var result = MoneyRounding.ToCurrency(42.565m, RoundingStrategy.ToEven);

        Assert.Equal(42.56m, result);
    }

    [Fact]
    public void ToCurrency_WhenValueAlreadyHasTwoDecimalPlaces_ReturnsSameValue()
    {
        var result = MoneyRounding.ToCurrency(42.50m);

        Assert.Equal(42.50m, result);
    }
}
