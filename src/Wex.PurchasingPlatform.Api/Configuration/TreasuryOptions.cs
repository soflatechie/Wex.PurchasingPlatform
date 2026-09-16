namespace Wex.PurchasingPlatform.Api.Configuration;

public class TreasuryOptions
{
    public const string SectionName = "Treasury";

    public string BaseUrl { get; set; } = string.Empty;

    public string RatesOfExchangeEndpoint { get; set; } = string.Empty;

    public int CurrencyOptionsPageSize { get; set; }
}
