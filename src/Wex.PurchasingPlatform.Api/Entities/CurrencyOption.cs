namespace Wex.PurchasingPlatform.Api.Entities;

public class CurrencyOption
{
    public int Id { get; set; }

    public string Country { get; set; } = string.Empty;

    public string CurrencyName { get; set; } = string.Empty;
}
