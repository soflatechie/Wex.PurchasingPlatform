using System.Text.Json.Serialization;

namespace Wex.PurchasingPlatform.Api.ExternalServices.Implementation;

public class TreasuryRatesOfExchangeResponse
{
    [JsonPropertyName("data")]
    public List<TreasuryRateOfExchangeRow> Data { get; set; } = [];
}

public class TreasuryRateOfExchangeRow
{
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("record_date")]
    public string RecordDate { get; set; } = string.Empty;

    [JsonPropertyName("exchange_rate")]
    public string ExchangeRate { get; set; } = string.Empty;
}
