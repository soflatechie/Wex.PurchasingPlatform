namespace Wex.PurchasingPlatform.Api.Services.Implementation;

public record RateSelectionResult(
    decimal ExchangeRate,
    DateOnly RateDate,
    bool IsStale);
