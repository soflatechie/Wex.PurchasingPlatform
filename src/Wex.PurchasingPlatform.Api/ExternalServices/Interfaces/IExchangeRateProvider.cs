using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Api.ExternalServices.Interfaces;

public interface IExchangeRateProvider
{
    Task<ExchangeRateLookupResult?> GetLatestRateOnOrBeforeAsync(string country, string currencyName, DateOnly onOrBeforeDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CurrencyOptionDto>> FetchAllCurrencyOptionsAsync(CancellationToken cancellationToken = default);
}
