namespace Wex.PurchasingPlatform.Api.Common;

public enum RoundingStrategy
{
    AwayFromZero,
    ToEven,
    Up,
    Down
}

public static class MoneyRounding
{
    private const int DecimalPlaces = 2;

    public const RoundingStrategy DefaultStrategy = RoundingStrategy.AwayFromZero;

    public static decimal ToCurrency(decimal value, RoundingStrategy strategy = DefaultStrategy)
    {
        return strategy switch
        {
            RoundingStrategy.AwayFromZero => Math.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero),
            RoundingStrategy.ToEven => Math.Round(value, DecimalPlaces, MidpointRounding.ToEven),
            RoundingStrategy.Up => Math.Ceiling(value * 100) / 100,
            RoundingStrategy.Down => Math.Truncate(value * 100) / 100,
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unsupported rounding strategy.")
        };
    }
}
