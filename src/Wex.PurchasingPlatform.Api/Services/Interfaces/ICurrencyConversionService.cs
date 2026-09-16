using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Api.Services.Interfaces;

public interface ICurrencyConversionService
{
    Task<ConvertedPurchaseTransactionDto?> GetConvertedAsync(Guid transactionId, string country, string currencyName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAvailableCountriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CurrencyOptionDto>> GetAvailableCurrenciesAsync(string country, DateOnly transactionDate, CancellationToken cancellationToken = default);
}
