using System.Globalization;
using Microsoft.Extensions.Options;
using Wex.PurchasingPlatform.Api.Common.Exceptions;
using Wex.PurchasingPlatform.Api.Configuration;
using Wex.PurchasingPlatform.Api.ExternalServices.Interfaces;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Api.ExternalServices.Implementation;

public class TreasuryExchangeRateClient(HttpClient httpClient, IOptions<TreasuryOptions> options) : IExchangeRateProvider
{
    private readonly TreasuryOptions _options = options.Value;

    public async Task<ExchangeRateLookupResult?> GetLatestRateOnOrBeforeAsync(string country, string currencyName, DateOnly onOrBeforeDate, CancellationToken cancellationToken = default)
    {
        var countryCurrencyDesc = $"{country}-{currencyName}";
        var filter = $"country_currency_desc:eq:{countryCurrencyDesc},record_date:lte:{onOrBeforeDate:yyyy-MM-dd}";
        var requestUri = $"{_options.RatesOfExchangeEndpoint}?fields=country,currency,record_date,exchange_rate&filter={Uri.EscapeDataString(filter)}&sort=-record_date&page[size]=1";

        var payload = await GetAsync<TreasuryRatesOfExchangeResponse>(requestUri, cancellationToken);

        var row = payload.Data.FirstOrDefault();
        if (row is null)
            return null;

        return new ExchangeRateLookupResult(
            row.Country,
            row.Currency,
            DateOnly.Parse(row.RecordDate, CultureInfo.InvariantCulture),
            decimal.Parse(row.ExchangeRate, CultureInfo.InvariantCulture));
    }

    public async Task<IReadOnlyList<CurrencyOptionDto>> FetchAllCurrencyOptionsAsync(CancellationToken cancellationToken = default)
    {
        var currencyOptions = new List<CurrencyOptionDto>();
        var pageNumber = 1;

        while (true)
        {
            var requestUri = $"{_options.RatesOfExchangeEndpoint}?fields=country,currency,country_currency_desc&sort=country_currency_desc&page[number]={pageNumber}&page[size]={_options.CurrencyOptionsPageSize}";
            var payload = await GetAsync<TreasuryRatesOfExchangeResponse>(requestUri, cancellationToken);

            if (payload.Data.Count == 0)
                break;

            currencyOptions.AddRange(payload.Data.Select(row => new CurrencyOptionDto(row.Country, row.Currency)));

            if (payload.Data.Count < _options.CurrencyOptionsPageSize)
                break;

            pageNumber++;
        }

        return currencyOptions
            .DistinctBy(option => (option.Country, option.CurrencyName))
            .ToList();
    }

    private async Task<TResponse> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            if (payload is null)
                throw new ExchangeRateProviderUnavailableException("Treasury returned an empty response.", new InvalidOperationException("Response body deserialized to null."));

            return payload;
        }
        catch (HttpRequestException ex)
        {
            throw new ExchangeRateProviderUnavailableException("The Treasury exchange rate service is unreachable.", ex);
        }
    }
}
