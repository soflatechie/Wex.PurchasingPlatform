using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Wex.PurchasingPlatform.Api.Common.Exceptions;
using Wex.PurchasingPlatform.Api.Configuration;
using Wex.PurchasingPlatform.Api.ExternalServices.Implementation;
using Wex.PurchasingPlatform.Tests.TestDoubles;

namespace Wex.PurchasingPlatform.Tests.ExternalServices;

public class TreasuryExchangeRateClientTests
{
    private static readonly TreasuryOptions TreasuryOptions = new()
    {
        BaseUrl = "https://api.fiscaldata.treasury.gov/services/api/fiscal_service/",
        RatesOfExchangeEndpoint = "v1/accounting/od/rates_of_exchange",
        CurrencyOptionsPageSize = 1000
    };

    [Fact]
    public async Task GetLatestRateOnOrBeforeAsync_WhenTreasuryReturnsARow_MapsItToLookupResult()
    {
        const string fixture = """
            {
                "data": [
                    {
                        "country": "Canada",
                        "currency": "Dollar",
                        "record_date": "2026-01-15",
                        "exchange_rate": "1.352"
                    }
                ]
            }
            """;

        var client = CreateClient(JsonResponder(fixture));

        var result = await client.GetLatestRateOnOrBeforeAsync("Canada", "Dollar", new DateOnly(2026, 1, 31));

        Assert.NotNull(result);
        Assert.Equal("Canada", result!.Country);
        Assert.Equal("Dollar", result.CurrencyName);
        Assert.Equal(new DateOnly(2026, 1, 15), result.RecordDate);
        Assert.Equal(1.352m, result.ExchangeRate);
    }

    [Fact]
    public async Task GetLatestRateOnOrBeforeAsync_WhenTreasuryReturnsNoRows_ReturnsNull()
    {
        var client = CreateClient(JsonResponder("""{ "data": [] }"""));

        var result = await client.GetLatestRateOnOrBeforeAsync("Atlantis", "Denarius", new DateOnly(2026, 1, 31));

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestRateOnOrBeforeAsync_WhenTreasuryIsUnreachable_ThrowsExchangeRateProviderUnavailableException()
    {
        var client = CreateClient(_ => throw new HttpRequestException("connection refused"));

        await Assert.ThrowsAsync<ExchangeRateProviderUnavailableException>(
            () => client.GetLatestRateOnOrBeforeAsync("Canada", "Dollar", new DateOnly(2026, 1, 31)));
    }

    [Fact]
    public async Task GetLatestRateOnOrBeforeAsync_FiltersByCountryCurrencyDescAndDateOnOrBefore()
    {
        var handler = new FakeHttpMessageHandler(JsonResponder("""{ "data": [] }"""));
        var client = new TreasuryExchangeRateClient(new HttpClient(handler) { BaseAddress = new Uri(TreasuryOptions.BaseUrl) }, Options.Create(TreasuryOptions));

        await client.GetLatestRateOnOrBeforeAsync("Canada", "Dollar", new DateOnly(2026, 1, 31));

        var query = Uri.UnescapeDataString(handler.LastRequest!.RequestUri!.Query);
        Assert.Contains("filter=country_currency_desc:eq:Canada-Dollar,record_date:lte:2026-01-31", query);
        Assert.Contains("sort=-record_date", query);
        Assert.Contains("page[size]=1", query);
    }

    [Fact]
    public async Task FetchAllCurrencyOptionsAsync_MapsRowsToDistinctCurrencyOptions()
    {
        const string fixture = """
            {
                "data": [
                    { "country": "Canada", "currency": "Dollar" },
                    { "country": "United Kingdom", "currency": "Pound" }
                ]
            }
            """;

        var client = CreateClient(JsonResponder(fixture));

        var result = await client.FetchAllCurrencyOptionsAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, option => option.Country == "Canada" && option.CurrencyName == "Dollar");
        Assert.Contains(result, option => option.Country == "United Kingdom" && option.CurrencyName == "Pound");
    }

    private static Func<HttpRequestMessage, HttpResponseMessage> JsonResponder(string json)
    {
        return _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static TreasuryExchangeRateClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(responder)) { BaseAddress = new Uri(TreasuryOptions.BaseUrl) };

        return new TreasuryExchangeRateClient(httpClient, Options.Create(TreasuryOptions));
    }
}
