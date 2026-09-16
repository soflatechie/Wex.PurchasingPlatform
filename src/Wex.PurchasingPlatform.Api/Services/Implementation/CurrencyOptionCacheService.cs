using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.ExternalServices.Interfaces;
using Wex.PurchasingPlatform.Api.Repositories.Implementation;
using Wex.PurchasingPlatform.Api.Services.Interfaces;

namespace Wex.PurchasingPlatform.Api.Services.Implementation;

public class CurrencyOptionCacheService(
    CurrencyOptionRepository currencyOptionRepository,
    IExchangeRateProvider exchangeRateProvider,
    ILogger<CurrencyOptionCacheService> logger) : ICurrencyOptionCacheService
{
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var currencyOptions = await exchangeRateProvider.FetchAllCurrencyOptionsAsync(cancellationToken);

        var entities = currencyOptions
            .Select(option => new CurrencyOption { Country = option.Country, CurrencyName = option.CurrencyName })
            .ToList();

        await currencyOptionRepository.ReplaceAllAsync(entities, cancellationToken);

        logger.LogInformation("Currency identity cache refreshed: {Count} pairs.", entities.Count);
    }
}
