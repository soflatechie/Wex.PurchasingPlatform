namespace Wex.PurchasingPlatform.Api.ExternalServices;

public record ExchangeRateLookupResult(
    string Country,
    string CurrencyName,
    DateOnly RecordDate,
    decimal ExchangeRate);
