namespace Wex.PurchasingPlatform.Api.Entities;

public class ExchangeRateQuote
{
    public int Id { get; set; }

    public string Country { get; set; } = string.Empty;

    public string CurrencyName { get; set; } = string.Empty;

    public DateOnly RecordDate { get; set; }

    public decimal ExchangeRate { get; set; }

    public DateTime FetchedAtUtc { get; set; }
}
